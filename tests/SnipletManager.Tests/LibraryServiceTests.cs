using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SnipletManager.Core;
using SnipletManager.Core.Data;
using Xunit;

namespace SnipletManager.Tests;

public sealed class LibraryServiceTests : IAsyncLifetime
{
    private readonly string path = Path.Combine(Path.GetTempPath(), $"sniplet-test-{Guid.NewGuid():N}.db");
    private PooledDbContextFactory<AppDbContext> factory = null!;
    private LibraryService service = null!;
    private AppSettings settings = null!;

    public async Task InitializeAsync()
    {
        settings = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json").Build().GetSection("AppSettings").Get<AppSettings>()!;
        factory = new PooledDbContextFactory<AppDbContext>(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={path};Pooling=False;Foreign Keys=True").Options);
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
        service = new LibraryService(factory, Options.Create(settings));
    }

    public Task DisposeAsync()
    {
        File.Delete(path);
        File.Delete(path + "-wal");
        File.Delete(path + "-shm");
        return Task.CompletedTask;
    }

    private async Task<CategoryItem> AddCategoryAsync(string name = "Work")
    {
        await service.SaveCategoryAsync(null, name, Guid.Empty);
        return (await service.GetCategoriesAsync()).Single(x => x.Name == name.Trim());
    }

    private Task<SnippetPage> ReadAsync(int? categoryId = null, string? search = null,
        SnippetSort sort = SnippetSort.Name, bool descending = false, int start = 0, int? count = null) =>
        service.GetSnippetsAsync(categoryId, search, sort, descending, start, count ?? settings.PageSize);

    [Fact]
    public async Task SaveSnippet_PreservesWhitespaceAndSurvivesNewServiceInstance()
    {
        // Arrange
        var category = await AddCategoryAsync();
        var text = "  Hello\r\n\tworld!  \n";
        // Act
        await service.SaveSnippetAsync(null, category.Id, "  Greeting  ", text, Guid.Empty);
        var reopened = new LibraryService(factory, Options.Create(settings));
        var page = await reopened.GetSnippetsAsync(category.Id, null, SnippetSort.Name, false, 0, settings.PageSize);
        // Assert
        var snippet = Assert.Single(page.Items);
        Assert.Equal("Greeting", snippet.Name);
        Assert.Equal(text, snippet.Value);
        Assert.Equal(1, Assert.Single(await service.GetCategoriesAsync()).Count);
        Assert.False(snippet.IsMasked);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_PersistMaskWithoutEncryptingText_When_SavingSnippet(bool isMasked)
    {
        // Arrange
        var category = await AddCategoryAsync();
        var text = "  Private\r\n\ttext!  \n";

        // Act
        await service.SaveSnippetAsync(null, category.Id, "Masked snippet", text, Guid.Empty, isMasked);
        var reopened = new LibraryService(factory, Options.Create(settings));
        var page = await reopened.GetSnippetsAsync(null, null, SnippetSort.Name, false, 0, settings.PageSize);

        // Assert
        var snippet = Assert.Single(page.Items);
        Assert.Equal(isMasked, snippet.IsMasked);
        Assert.Equal(text, snippet.Value);
        await using var db = await factory.CreateDbContextAsync();
        var entity = await db.Snippets.SingleAsync();
        Assert.Equal(isMasked, entity.IsMasked);
        Assert.Equal(text, entity.Value);
    }

    [Fact]
    public async Task Should_UpdateMaskAndRejectStaleChanges_When_EditingSnippet()
    {
        // Arrange
        var category = await AddCategoryAsync();
        await service.SaveSnippetAsync(null, category.Id, "Name", "Original", Guid.Empty);
        var original = Assert.Single((await ReadAsync()).Items);

        // Act
        await service.SaveSnippetAsync(original.Id, category.Id, "Name", original.Value, original.Version, true);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(original.Id, category.Id, "Name", "Stale", original.Version, false));

        // Assert
        var masked = Assert.Single((await ReadAsync()).Items);
        Assert.True(masked.IsMasked);
        Assert.Equal("Original", masked.Value);
        await service.SaveSnippetAsync(masked.Id, category.Id, masked.Name, masked.Value, masked.Version, false);
        var unmasked = Assert.Single((await ReadAsync()).Items);
        Assert.False(unmasked.IsMasked);
        Assert.Equal("Original", unmasked.Value);
    }

    [Fact]
    public async Task Should_PreserveExistingTextAndDefaultToUnmasked_When_UpgradingDatabase()
    {
        // Arrange
        await using var db = await factory.CreateDbContextAsync();
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261107182936_InitialLibrary");
        var categoryVersion = Guid.NewGuid();
        var snippetVersion = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO Categories (Id, Name, NormalizedName, Version) VALUES (1, {"Work"}, {"WORK"}, {categoryVersion})");
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO Snippets (CategoryId, Name, Value, NormalizedName, SearchValue, Version) VALUES (1, {"Existing"}, {"Existing\ntext"}, {"EXISTING"}, {"EXISTING\nTEXT"}, {snippetVersion})");

        // Act
        await db.Database.MigrateAsync();

        // Assert
        var snippet = Assert.Single((await ReadAsync()).Items);
        Assert.False(snippet.IsMasked);
        Assert.Equal("Existing\ntext", snippet.Value);
        Assert.Equal(snippetVersion, snippet.Version);
    }

    [Fact]
    public async Task Should_SearchAndSortOriginalText_When_SnippetsAreMasked()
    {
        // Arrange
        var category = await AddCategoryAsync();
        await service.SaveSnippetAsync(null, category.Id, "One", "Zulu text", Guid.Empty, true);
        await service.SaveSnippetAsync(null, category.Id, "Two", "Alpha text", Guid.Empty, true);

        // Act
        var sorted = await ReadAsync(sort: SnippetSort.Value);
        var searched = await ReadAsync(search: "zulu");

        // Assert
        Assert.Equal(new[] { "Two", "One" }, sorted.Items.Select(x => x.Name));
        Assert.All(sorted.Items, snippet => Assert.True(snippet.IsMasked));
        var match = Assert.Single(searched.Items);
        Assert.Equal("One", match.Name);
        Assert.Equal("Zulu text", match.Value);
        Assert.True(match.IsMasked);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SaveCategory_RejectsMissingNames(string? name)
    {
        await Assert.ThrowsAsync<ValidationException>(() => service.SaveCategoryAsync(null, name, Guid.Empty));
        Assert.Empty(await service.GetCategoriesAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \r\n\t")]
    public async Task SaveSnippet_RejectsMissingText(string? text)
    {
        var category = await AddCategoryAsync();
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(null, category.Id, "Greeting", text, Guid.Empty));
        Assert.Empty((await ReadAsync()).Items);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SaveSnippet_RejectsMissingNames(string? name)
    {
        var category = await AddCategoryAsync();
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(null, category.Id, name, "Text", Guid.Empty));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Save_EnforcesConfiguredLengthBoundaries(bool isMasked)
    {
        var category = await AddCategoryAsync(new string('a', settings.CategoryNameLimit));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveCategoryAsync(null, new string('b', settings.CategoryNameLimit + 1), Guid.Empty));
        await service.SaveSnippetAsync(null, category.Id, new string('n', settings.SnippetNameLimit),
            new string('t', settings.SnippetTextLimit), Guid.Empty, isMasked);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(null, category.Id, new string('n', settings.SnippetNameLimit + 1), "Text", Guid.Empty));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(null, category.Id, "Oversized", new string('t', settings.SnippetTextLimit + 1), Guid.Empty, isMasked));
        Assert.Single((await ReadAsync()).Items);
    }

    [Fact]
    public async Task Save_RejectsCaseInsensitiveDuplicatesButAllowsSameNameInDifferentCategories()
    {
        var work = await AddCategoryAsync();
        var personal = await AddCategoryAsync("Personal");
        await Assert.ThrowsAsync<ValidationException>(() => service.SaveCategoryAsync(null, " work ", Guid.Empty));
        await service.SaveSnippetAsync(null, work.Id, "Welcome", "A", Guid.Empty);
        await Assert.ThrowsAsync<ValidationException>(() => service.SaveSnippetAsync(null, work.Id, " welcome ", "B", Guid.Empty));
        await service.SaveSnippetAsync(null, personal.Id, "Welcome", "C", Guid.Empty);
        Assert.Equal(2, (await ReadAsync()).Total);
    }

    [Fact]
    public async Task EditSnippet_UpdatesTextAndMovesCategory()
    {
        var work = await AddCategoryAsync();
        var personal = await AddCategoryAsync("Personal");
        await service.SaveSnippetAsync(null, work.Id, "Original", "A", Guid.Empty);
        var original = Assert.Single((await ReadAsync()).Items);
        await service.SaveSnippetAsync(original.Id, personal.Id, "Renamed", "B", original.Version);
        var updated = Assert.Single((await ReadAsync(personal.Id)).Items);
        Assert.Equal(original.Id, updated.Id);
        Assert.Equal("Renamed", updated.Name);
        Assert.Equal("B", updated.Value);
        Assert.NotEqual(original.Version, updated.Version);
        Assert.Empty((await ReadAsync(work.Id)).Items);
    }

    [Fact]
    public async Task EditCategory_UpdatesSidebarAndGridCategoryNames()
    {
        var category = await AddCategoryAsync();
        await service.SaveSnippetAsync(null, category.Id, "Name", "Value", Guid.Empty);
        await service.SaveCategoryAsync(category.Id, "Office", category.Version);
        Assert.Equal("Office", Assert.Single(await service.GetCategoriesAsync()).Name);
        Assert.Equal("Office", Assert.Single((await ReadAsync()).Items).CategoryName);
    }

    [Fact]
    public async Task DeleteCategory_CascadesOnlyItsSnippets()
    {
        var work = await AddCategoryAsync();
        var personal = await AddCategoryAsync("Personal");
        await service.SaveSnippetAsync(null, work.Id, "One", "A", Guid.Empty);
        await service.SaveSnippetAsync(null, personal.Id, "Two", "B", Guid.Empty);
        await service.DeleteCategoryAsync(work.Id, work.Version);
        Assert.Equal(personal.Id, Assert.Single(await service.GetCategoriesAsync()).Id);
        Assert.Equal("Two", Assert.Single((await ReadAsync()).Items).Name);
    }

    [Fact]
    public async Task DeleteSnippet_RemovesSnippetButKeepsCategory()
    {
        var category = await AddCategoryAsync();
        await service.SaveSnippetAsync(null, category.Id, "Name", "Value", Guid.Empty);
        var snippet = Assert.Single((await ReadAsync()).Items);
        await service.DeleteSnippetAsync(snippet.Id, snippet.Version);
        Assert.Empty((await ReadAsync()).Items);
        Assert.Equal(0, Assert.Single(await service.GetCategoriesAsync()).Count);
    }

    [Theory]
    [InlineData("Greeting", "Hi there", "gREet")]
    [InlineData("Example", "Hi THERE", "there")]
    [InlineData("Café", "Résumé", "RÉSUMÉ")]
    [InlineData("Percent", "100%_complete", "%_")]
    [InlineData("<script>", "'; DROP TABLE Snippets; --", "DROP TABLE")]
    public async Task Search_MatchesLiteralNamesAndTextIgnoringCase(string name, string text, string search)
    {
        var category = await AddCategoryAsync();
        await service.SaveSnippetAsync(null, category.Id, name, text, Guid.Empty);
        Assert.Single((await ReadAsync(category.Id, search)).Items);
        Assert.Empty((await ReadAsync(category.Id, "not-a-match")).Items);
        Assert.Single(await service.GetCategoriesAsync());
    }

    [Fact]
    public async Task Search_RespectsCategoryScopeAndAllSnippetsView()
    {
        var work = await AddCategoryAsync();
        var personal = await AddCategoryAsync("Personal");
        await service.SaveSnippetAsync(null, work.Id, "Greeting", "A", Guid.Empty);
        await service.SaveSnippetAsync(null, personal.Id, "Greeting", "B", Guid.Empty);
        Assert.Single((await ReadAsync(work.Id, "greet")).Items);
        Assert.Equal(2, (await ReadAsync(null, "greet")).Total);
    }

    [Theory]
    [InlineData(SnippetSort.Name, false, "Alpha")]
    [InlineData(SnippetSort.Name, true, "Zulu")]
    [InlineData(SnippetSort.Value, false, "Zulu")]
    [InlineData(SnippetSort.Value, true, "Alpha")]
    [InlineData(SnippetSort.CategoryName, false, "Zulu")]
    [InlineData(SnippetSort.CategoryName, true, "Alpha")]
    public async Task SortAndPagination_AppliesRequestedOrderAndReportsTotal(SnippetSort sort, bool descending, string first)
    {
        var work = await AddCategoryAsync();
        var personal = await AddCategoryAsync("Personal");
        await service.SaveSnippetAsync(null, work.Id, "Alpha", "Zulu text", Guid.Empty);
        await service.SaveSnippetAsync(null, personal.Id, "Zulu", "Alpha text", Guid.Empty);
        var page = await ReadAsync(sort: sort, descending: descending, count: 1);
        Assert.Equal(2, page.Total);
        Assert.Equal(first, Assert.Single(page.Items).Name);
        var next = await ReadAsync(sort: sort, descending: descending, start: 1, count: 1);
        Assert.NotEqual(first, Assert.Single(next.Items).Name);
    }

    [Fact]
    public async Task Save_RejectsMissingCategoriesAndDeletedItems()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(null, 999, "Name", "Text", Guid.Empty));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveCategoryAsync(999, "Name", Guid.NewGuid()));
        var category = await AddCategoryAsync();
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(999, category.Id, "Name", "Text", Guid.NewGuid()));
        await Assert.ThrowsAsync<ValidationException>(() => service.DeleteCategoryAsync(999, Guid.NewGuid()));
        await Assert.ThrowsAsync<ValidationException>(() => service.DeleteSnippetAsync(999, Guid.NewGuid()));
    }

    [Fact]
    public async Task EditAndDelete_RejectStaleVersionsWithoutOverwritingData()
    {
        var category = await AddCategoryAsync();
        await service.SaveSnippetAsync(null, category.Id, "Name", "Original", Guid.Empty);
        var original = Assert.Single((await ReadAsync()).Items);
        await service.SaveSnippetAsync(original.Id, category.Id, "Name", "Updated", original.Version);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveSnippetAsync(original.Id, category.Id, "Name", "Stale", original.Version));
        await Assert.ThrowsAsync<ValidationException>(() => service.DeleteSnippetAsync(original.Id, original.Version));
        await service.SaveCategoryAsync(category.Id, "New name", category.Version);
        await Assert.ThrowsAsync<ValidationException>(() => service.DeleteCategoryAsync(category.Id, category.Version));
        Assert.Equal("Updated", Assert.Single((await ReadAsync()).Items).Value);
    }

    [Fact]
    public async Task ConcurrentReads_UseIndependentContexts()
    {
        var category = await AddCategoryAsync();
        await service.SaveSnippetAsync(null, category.Id, "Name", "Value", Guid.Empty);
        var reads = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => ReadAsync()));
        Assert.All(reads, page => Assert.Single(page.Items));
    }

    [Fact]
    public async Task CancelledRead_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetSnippetsAsync(null, null, SnippetSort.Name, false, 0, settings.PageSize, cancellation.Token));
    }

    [Fact]
    public async Task Read_RejectsInvalidPaginationSortAndOversizedSearch()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ReadAsync(start: -1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ReadAsync(count: 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ReadAsync(count: settings.PageSize + 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ReadAsync(sort: (SnippetSort)999));
        await Assert.ThrowsAsync<ValidationException>(() => ReadAsync(search: new string('x', settings.SearchLimit + 1)));
    }
}
