using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SnipletManager.Core;
using SnipletManager.Core.Data;
using SnipletManager.Desktop.Components;
using SnipletManager.Desktop.Services;
using Xunit;

namespace SnipletManager.Tests;

public sealed class LibraryComponentTests : BunitContext, IAsyncLifetime
{
    private readonly string path = Path.Combine(Path.GetTempPath(), $"sniplet-ui-test-{Guid.NewGuid():N}.db");

    public LibraryComponentTests()
    {
        var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();
        Services.AddOptions<AppSettings>().Bind(config.GetSection("AppSettings"));
        Services.AddDbContextFactory<AppDbContext>(builder => builder.UseSqlite($"Data Source={path};Pooling=False;Foreign Keys=True"));
        Services.AddScoped<LibraryService>();
        Services.AddSingleton<ClipboardService>();
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule().Mode = JSRuntimeMode.Loose;
    }

    public async Task InitializeAsync()
    {
        await using var db = await Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        File.Delete(path);
        File.Delete(path + "-wal");
        File.Delete(path + "-shm");
    }

    private async Task SeedAsync(string category = "Work", string name = "Greeting", string text = "Hello world")
    {
        var service = Services.GetRequiredService<LibraryService>();
        await service.SaveCategoryAsync(null, category, Guid.Empty);
        var item = (await service.GetCategoriesAsync()).Single(x => x.Name == category);
        await service.SaveSnippetAsync(null, item.Id, name, text, Guid.Empty);
    }

    [Fact]
    public void EmptyLibrary_ShowsCategoryCreationAndDisablesSnippetCreation()
    {
        var cut = Render<Library>();
        cut.WaitForAssertion(() => Assert.Contains("Create a category", cut.Find(".empty-state").TextContent));
        Assert.True(cut.Find(".toolbar button.primary").HasAttribute("disabled"));
    }

    [Fact]
    public async Task DatabaseFailure_ShowsErrorInsteadOfEmptyLibraryMessage()
    {
        await using (var db = await Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            await db.Database.ExecuteSqlRawAsync("DROP TABLE Snippets");
        var cut = Render<Library>();
        cut.WaitForAssertion(() => Assert.Contains("could not be loaded", cut.Find(".error-banner").TextContent));
        Assert.Empty(cut.FindAll(".empty-state h2"));
        Assert.Single(cut.FindAll(".error-toast"));
    }

    [Fact]
    public async Task SnippetText_RendersAsTextNotExecutableMarkup()
    {
        await SeedAsync(name: "<img src=x onerror=alert(1)>", text: "<script>alert(1)</script>");
        var cut = Render<Library>();
        cut.WaitForAssertion(() => Assert.Equal("<script>alert(1)</script>", cut.Find(".text-preview").TextContent));
        Assert.Empty(cut.FindAll("script, .snippet-grid img"));
    }

    [Fact]
    public void AddCategory_SavesAndDisplaysToast()
    {
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".empty-state h2").Count > 0);
        cut.Find("button[aria-label='Add category']").Click();
        cut.Find("#item-name").Input("Work");
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => Assert.Contains("Category added.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Contains("Work", cut.Find("nav").TextContent));
        Assert.Empty(cut.FindAll("form"));
    }

    [Fact]
    public async Task AddSnippet_SavesNameAndMultilineTextAndDisplaysToast()
    {
        await SeedAsync();
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".toolbar button.primary").Click();
        cut.Find("#item-name").Input("New snippet");
        cut.Find("#item-value").Input("Line 1\n  Line 2");
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => Assert.Contains("Snippet added.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".snippet-name").Count));
        Assert.Contains("Line 1\n  Line 2", cut.FindAll(".text-preview").Select(x => x.TextContent));
    }

    [Fact]
    public async Task EditSnippet_UpdatesGridAndDisplaysToast()
    {
        await SeedAsync();
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".snippet-name").Click();
        cut.Find("#item-name").Input("Changed");
        cut.Find("#item-value").Input("Updated text");
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => Assert.Contains("Snippet updated.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Equal("Changed", cut.Find(".snippet-name").TextContent));
        Assert.Equal("Updated text", cut.Find(".text-preview").TextContent);
    }

    [Fact]
    public async Task InvalidSnippet_ShowsErrorAndKeepsEditorOpen()
    {
        await SeedAsync();
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".toolbar button.primary").Click();
        cut.Find("#item-name").Input("Missing text");
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => Assert.Contains("Snippet text is required", cut.Find("form .error-banner").TextContent));
        Assert.Single(cut.FindAll("form"));
        Assert.Single(cut.FindAll(".snippet-name"));
    }

    [Fact]
    public async Task Search_FiltersNameAndTextWithinSelectedCategory()
    {
        await SeedAsync("Work", "Greeting", "Hello world");
        await SeedAsync("Personal", "Other", "HELLO AGAIN");
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 2);
        cut.FindAll("nav a").Single(x => x.TextContent.Contains("Work")).Click();
        cut.Find("#snippet-search").Input("world");
        cut.WaitForAssertion(() => Assert.Equal("Greeting", cut.Find(".snippet-name").TextContent));
        cut.Find("#snippet-search").Input("no match");
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".snippet-name")));
        cut.Find("nav a").Click();
        cut.Find("#snippet-search").Input("hello");
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".snippet-name").Count));
    }

    [Fact]
    public async Task DeleteSnippet_RequiresConfirmationAndDisplaysToast()
    {
        await SeedAsync();
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find("button[aria-label='Delete Greeting']").Click();
        Assert.Contains("cannot be undone", cut.Find(".delete-message").TextContent);
        Assert.Single(cut.FindAll(".snippet-name"));
        cut.Find(".dialog-footer button.danger").Click();
        cut.WaitForAssertion(() => Assert.Contains("Snippet deleted.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".snippet-name")));
    }

    [Fact]
    public async Task DeleteCategory_RequiresCascadeConfirmationAndUpdatesSidebar()
    {
        await SeedAsync();
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.FindAll("nav a").Single(x => x.TextContent.Contains("Work")).Click();
        cut.Find(".toolbar button.danger-outline").Click();
        Assert.Contains("every snippet currently in it", cut.Find(".delete-message").TextContent);
        cut.Find(".dialog-footer button.danger").Click();
        cut.WaitForAssertion(() => Assert.Contains("Category and its snippets deleted.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.DoesNotContain("Work", cut.Find("nav").TextContent));
        Assert.Equal("All snippets", cut.Find("h1").TextContent);
    }
}
