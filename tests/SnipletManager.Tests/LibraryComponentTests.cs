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

    private async Task SeedAsync(string category = "Work", string name = "Greeting", string text = "Hello world", bool isMasked = false)
    {
        var service = Services.GetRequiredService<LibraryService>();
        await service.SaveCategoryAsync(null, category, Guid.Empty);
        var item = (await service.GetCategoriesAsync()).Single(x => x.Name == category);
        await service.SaveSnippetAsync(null, item.Id, name, text, Guid.Empty, isMasked);
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
    public async Task Should_DefaultToUnmasked_When_AddingSnippetAfterEditingMaskedSnippet()
    {
        // Arrange
        await SeedAsync(isMasked: true);
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".snippet-name").Click();
        Assert.True(cut.Find("#item-encrypt").HasAttribute("checked"));
        cut.Find(".dialog-footer button.secondary").Click();

        // Act
        cut.Find(".toolbar button.primary").Click();

        // Assert
        Assert.False(cut.Find("#item-encrypt").HasAttribute("checked"));
        Assert.Equal("TEXTAREA", cut.Find("#item-value").TagName);
    }

    [Fact]
    public async Task Should_SaveAndMaskText_When_EncryptIsSelectedForNewSnippet()
    {
        // Arrange
        await SeedAsync();
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".toolbar button.primary").Click();
        cut.Find("#item-name").Input("Hidden snippet");

        // Act
        cut.Find("#item-encrypt").Change(true);
        cut.Find("#item-value").Input("Private text");
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Snippet added.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".snippet-name").Count));
        Assert.DoesNotContain("Private text", cut.Find(".snippet-grid").OuterHtml);
        Assert.Single(cut.FindAll(".text-preview[aria-label='Hidden text']"));
        var service = Services.GetRequiredService<LibraryService>();
        var saved = (await service.GetSnippetsAsync(null, "Hidden snippet", SnippetSort.Name, false, 0, 20)).Items.Single();
        Assert.True(saved.IsMasked);
        Assert.Equal("Private text", saved.Value);
    }

    [Fact]
    public async Task Should_UsePasswordInputWithoutSpellingMirror_When_EditingMaskedSnippet()
    {
        // Arrange
        await SeedAsync(text: "<script>private\nvalue</script>", isMasked: true);
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);

        // Act
        cut.Find(".snippet-name").Click();

        // Assert
        Assert.Equal("password", cut.Find("#item-value").GetAttribute("type"));
        Assert.Equal("new-password", cut.Find("#item-value").GetAttribute("autocomplete"));
        Assert.Equal("false", cut.Find("#item-value").GetAttribute("spellcheck"));
        Assert.Null(cut.Find("#item-value").Closest(".spell-field"));
        Assert.Single(cut.FindAll(".spell-mirror")); // Only the name field is spell-checked.
        Assert.Equal("********", cut.Find(".text-preview").TextContent);
        Assert.DoesNotContain("private", cut.Find(".snippet-grid").OuterHtml);
        Assert.Empty(cut.FindAll("script"));
        Assert.Contains("stored unencrypted", cut.Find("#encrypt-help").TextContent);
    }

    [Fact]
    public async Task Should_PreserveMultilineText_When_TogglingMaskAndSaving()
    {
        // Arrange
        const string text = "  First line\r\n\tSecond line\n";
        await SeedAsync(text: text);
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".snippet-name").Click();

        // Act
        cut.Find("#item-encrypt").Change(true);
        Assert.Equal("password", cut.Find("#item-value").GetAttribute("type"));
        cut.Find("#item-encrypt").Change(false);
        Assert.Equal(text.Replace("\r\n", "\n"), cut.Find("#item-value").GetAttribute("value"));
        cut.Find("#item-encrypt").Change(true);
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Snippet updated.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".text-preview[aria-label='Hidden text']")));
        var service = Services.GetRequiredService<LibraryService>();
        var saved = (await service.GetSnippetsAsync(null, null, SnippetSort.Name, false, 0, 20)).Items.Single();
        Assert.Equal(text, saved.Value);
        cut.Find(".snippet-name").Click();
        Assert.True(cut.Find("#item-encrypt").HasAttribute("checked"));
        cut.Find("#item-encrypt").Change(false);
        Assert.Equal(text.Replace("\r\n", "\n"), cut.Find("#item-value").GetAttribute("value"));
    }

    [Fact]
    public async Task Should_UpdateTextAndRemoveMask_When_EncryptIsCleared()
    {
        // Arrange
        await SeedAsync(isMasked: true);
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".snippet-name").Click();

        // Act
        cut.Find("#item-encrypt").Change(false);
        cut.Find("#item-value").Input("Visible again\nNext line");
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Snippet updated.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Equal("Visible again\nNext line", cut.Find(".text-preview").TextContent));
        cut.Find(".snippet-name").Click();
        Assert.False(cut.Find("#item-encrypt").HasAttribute("checked"));
        Assert.Equal("TEXTAREA", cut.Find("#item-value").TagName);
    }

    [Fact]
    public async Task Should_SaveEditedPasswordText_When_MaskingRemainsEnabled()
    {
        // Arrange
        await SeedAsync(isMasked: true);
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".snippet-name").Click();

        // Act
        cut.Find("#item-value").Input("Updated hidden text");
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Snippet updated.", cut.Find(".toast").TextContent));
        cut.WaitForAssertion(() => Assert.Equal("********", cut.Find(".text-preview").TextContent));
        cut.Find(".snippet-name").Click();
        cut.Find("#item-encrypt").Change(false);
        Assert.Equal("Updated hidden text", cut.Find("#item-value").GetAttribute("value"));
    }

    [Fact]
    public async Task Should_KeepSavedMask_When_EditorChangesAreCancelled()
    {
        // Arrange
        await SeedAsync(isMasked: true);
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".snippet-name").Click();

        // Act
        cut.Find("#item-encrypt").Change(false);
        cut.Find(".dialog-footer button.secondary").Click();
        cut.Find(".snippet-name").Click();

        // Assert
        Assert.True(cut.Find("#item-encrypt").HasAttribute("checked"));
        Assert.Equal("password", cut.Find("#item-value").GetAttribute("type"));
        Assert.Equal("********", cut.Find(".text-preview").TextContent);
    }

    [Fact]
    public async Task Should_RejectEmptyMaskedText_When_SavingSnippet()
    {
        // Arrange
        await SeedAsync();
        var cut = Render<Library>();
        cut.WaitForState(() => cut.FindAll(".snippet-name").Count == 1);
        cut.Find(".toolbar button.primary").Click();
        cut.Find("#item-name").Input("Missing text");

        // Act
        cut.Find("#item-encrypt").Change(true);
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Snippet text is required", cut.Find("form .error-banner").TextContent));
        Assert.Equal("password", cut.Find("#item-value").GetAttribute("type"));
        Assert.True(cut.Find("#item-encrypt").HasAttribute("checked"));
        Assert.Single(cut.FindAll(".snippet-name"));
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
