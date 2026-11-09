# Database & ORM Standards

## 1. Technology Stack
- **Database Engine**: SQLite (Strictly local file-based).
- **ORM**: Entity Framework Core (EF Core).
- **Migrations**: Use EF Core Code-First Migrations.

## 2. Architecture Patterns
- **Context Management**
  - NEVER inject `DbContext` directly into Blazor components.
  - MUST use `IDbContextFactory<T>` for all database access to handle Blazor Server concurrency safely.
- **Service/Repository Boundary**
  - Do not access the database directly in Razor files.
  - Wrap all EF Core logic in scoped Services (e.g., `UserRepository`, `ProductService`).

## 3. Implementation Rules
- **Async Only**: All database operations must use `await` (e.g., `ToListAsync`, `FirstOrDefaultAsync`).
- **Connection String**: 
  - The connection string key is `"DefaultConnection"` by convention. This is **not a hardcoded literal** — it is a key read from `appsettings.json` via `IConfiguration`.
  - Register in `Program.cs` as: `builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));`
  - The actual connection string value (file path) lives only in `appsettings.json` and `appsettings.{Environment}.json`. Never hardcode it in C# source.
- **No Tracking**: Use `.AsNoTracking()` for all read-only queries to improve performance.
