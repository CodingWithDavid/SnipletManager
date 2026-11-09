using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SnipletManager.Core.Data;

namespace SnipletManager.Core;

public sealed record CategoryItem(int Id, string Name, int Count, Guid Version);
public sealed record SnippetItem(int Id, int CategoryId, string CategoryName, string Name, string Value, Guid Version, bool IsMasked = false);
public sealed record SnippetPage(IReadOnlyList<SnippetItem> Items, int Total);
public enum SnippetSort { Name, Value, CategoryName }

public sealed class LibraryService(IDbContextFactory<AppDbContext> factory, IOptions<AppSettings> options)
{
    private readonly AppSettings settings = options.Value;

    public async Task<IReadOnlyList<CategoryItem>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await db.Categories.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CategoryItem(x.Id, x.Name, x.Snippets.Count, x.Version))
            .ToListAsync(cancellationToken);
    }

    public async Task<SnippetPage> GetSnippetsAsync(int? categoryId, string? search, SnippetSort sort,
        bool descending, int start, int count, CancellationToken cancellationToken = default)
    {
        if (start < 0 || count < 1 || count > settings.PageSize)
            throw new ArgumentOutOfRangeException(nameof(count), "The requested page is invalid.");
        if (!Enum.IsDefined(sort))
            throw new ArgumentOutOfRangeException(nameof(sort));
        if (search?.Length > settings.SearchLimit)
            throw new ValidationException($"Search must be {settings.SearchLimit} characters or fewer.");
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var query = db.Snippets.AsNoTracking().AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.NormalizedName.Contains(term) || x.SearchValue.Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        IOrderedQueryable<Snippet> ordered = sort switch
        {
            SnippetSort.Value => descending ? query.OrderByDescending(x => x.Value) : query.OrderBy(x => x.Value),
            SnippetSort.CategoryName => descending ? query.OrderByDescending(x => x.Category.Name) : query.OrderBy(x => x.Category.Name),
            _ => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };
        var items = await ordered.ThenBy(x => x.Id).Skip(start).Take(count)
            .Select(x => new SnippetItem(x.Id, x.CategoryId, x.Category.Name, x.Name, x.Value, x.Version, x.IsMasked))
            .ToListAsync(cancellationToken);
        return new SnippetPage(items, total);
    }

    public async Task SaveCategoryAsync(int? id, string? name, Guid version, CancellationToken cancellationToken = default)
    {
        var cleanName = ValidateName(name, settings.CategoryNameLimit, "Category name");
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        Category entity;
        if (id.HasValue)
        {
            entity = await db.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new ValidationException("This category no longer exists. Refresh the library.");
            db.Entry(entity).Property(x => x.Version).OriginalValue = version;
        }
        else
        {
            entity = new Category();
            db.Categories.Add(entity);
        }
        entity.Name = cleanName;
        entity.NormalizedName = cleanName.ToUpperInvariant();
        entity.Version = Guid.NewGuid();
        await SaveAsync(db, "A category with this name already exists.", cancellationToken);
    }

    public async Task SaveSnippetAsync(int? id, int categoryId, string? name, string? value, Guid version,
        bool isMasked = false, CancellationToken cancellationToken = default)
    {
        var cleanName = ValidateName(name, settings.SnippetNameLimit, "Snippet name");
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("Snippet text is required.");
        if (value.Length > settings.SnippetTextLimit)
            throw new ValidationException($"Snippet text must be {settings.SnippetTextLimit:N0} characters or fewer.");
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        if (!await db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken))
            throw new ValidationException("Choose an existing category.");
        Snippet entity;
        if (id.HasValue)
        {
            entity = await db.Snippets.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new ValidationException("This snippet no longer exists. Refresh the library.");
            db.Entry(entity).Property(x => x.Version).OriginalValue = version;
        }
        else
        {
            entity = new Snippet();
            db.Snippets.Add(entity);
        }
        entity.CategoryId = categoryId;
        entity.Name = cleanName;
        entity.Value = value;
        entity.IsMasked = isMasked;
        entity.NormalizedName = cleanName.ToUpperInvariant();
        entity.SearchValue = value.ToUpperInvariant();
        entity.Version = Guid.NewGuid();
        await SaveAsync(db, "A snippet with this name already exists in this category.", cancellationToken);
    }

    public async Task DeleteCategoryAsync(int id, Guid version, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var entity = await db.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ValidationException("This category has already been deleted.");
        db.Entry(entity).Property(x => x.Version).OriginalValue = version;
        db.Categories.Remove(entity);
        await SaveAsync(db, "The category could not be deleted.", cancellationToken);
    }

    public async Task DeleteSnippetAsync(int id, Guid version, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var entity = await db.Snippets.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ValidationException("This snippet has already been deleted.");
        db.Entry(entity).Property(x => x.Version).OriginalValue = version;
        db.Snippets.Remove(entity);
        await SaveAsync(db, "The snippet could not be deleted.", cancellationToken);
    }

    private static string ValidateName(string? name, int limit, string label)
    {
        var clean = name?.Trim();
        if (string.IsNullOrEmpty(clean))
            throw new ValidationException($"{label} is required.");
        if (clean.Length > limit)
            throw new ValidationException($"{label} must be {limit} characters or fewer.");
        return clean;
    }

    private static async Task SaveAsync(AppDbContext db, string duplicateMessage, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ValidationException("This item changed in another window. Refresh before editing it again.", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            throw new ValidationException(duplicateMessage, exception);
        }
    }
}
