using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SnipletManager.Core.Data;

public sealed class Category
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";
    public Guid Version { get; set; }
    public List<Snippet> Snippets { get; set; } = [];
}

public sealed class Snippet
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Value { get; set; } = "";
    public string NormalizedName { get; set; } = "";
    public string SearchValue { get; set; } = "";
    public Guid Version { get; set; }
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Snippet> Snippets => Set<Snippet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasIndex(x => x.NormalizedName).IsUnique();
        modelBuilder.Entity<Category>().Property(x => x.Version).IsConcurrencyToken();
        modelBuilder.Entity<Snippet>().HasIndex(x => new { x.CategoryId, x.NormalizedName }).IsUnique();
        modelBuilder.Entity<Snippet>().Property(x => x.Version).IsConcurrencyToken();
        modelBuilder.Entity<Snippet>().HasOne(x => x.Category).WithMany(x => x.Snippets)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}
