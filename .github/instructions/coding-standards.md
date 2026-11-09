# Blazor Server Coding Standards

## 1. Architecture & Rendering
- **Framework**: Target .NET 10.
- **Render Mode**: We strictly use Interactive server-side rendering (interactive SSR). You must apply `@rendermode InteractiveServer` globally or at the component level [6, 7].
- **Separation of Concerns**: Razor file code should only be for UI support [9]. Keep `@code` blocks minimal and delegate all complex business logic to injected services.


## 2. Component Usage Rules (STRICT)
- **Forms & Inputs**: DO NOT use built-in Blazor components like `<EditForm>`, `<InputText>`, or `<InputSelect>`.
  - **USE**: Raw HTML tags (`<form>`, `<input>`, `<select>`).
  - **Binding**: Use standard `@bind` directive on raw HTML elements.
  - **Validation**: Manually display validation messages using `@if` blocks or a custom error summary component.
- **Data Grids**: DO NOT use HTML tables for complex data.
  - **USE**: `Microsoft.AspNetCore.Components.QuickGrid` for all tabular data.
  - Ensure pagination and sorting are enabled on the QuickGrid.

## 3. State Management
- Use Cascading Parameters for global state (Theme, User).
- Use Scoped Services for page-level state.

## 4. Coding
- Use IDbContextFactory
- No warnings on check-in
- Validate user inputs
- No CSS in razor files
- **Configuration over constants**: All configurable values (timeouts, page sizes, feature flags, retry counts, API endpoints) MUST be defined in `appsettings.json` and injected via `IOptions<T>`. Never use C# `const`, `static readonly`, or inline literals for values that could change per environment.
  ```csharp
  // ✅ CORRECT
  public class ProductService(IOptions<ProductSettings> options) { ... }

  // ❌ WRONG
  private const int PageSize = 20;
  private const string ApiEndpoint = "https://api.example.com";
  ```
  See `.github/instructions/configuration-standards.md` for the full pattern.

## 5. UI & Frontend Rules
- **Isolated CSS**: You MUST use Isolated CSS (e.g., `Component.razor.css`). Do not place CSS directly inside Razor files.
- **JavaScript**: Do not add custom JavaScript interop unless absolutely necessary, and prefer C#.

## 6. Resource Management (Memory Leak Prevention)

These rules are **non-negotiable**. Violations cause memory leaks in Blazor Server's long-lived circuit model.

### DbContext Lifetime
- **NEVER** store a `DbContext` or `IDbContextFactory`-created context as a component field.
- **ALWAYS** use `await using` so the context is disposed immediately after use:
  ```csharp
  // ✅ CORRECT
  await using var context = await _factory.CreateDbContextAsync(cancellationToken);
  var items = await context.Products.AsNoTracking().ToListAsync(cancellationToken);

  // ❌ WRONG — context stored as field leaks on reconnect
  private AppDbContext _context = null!;
  ```

### Component Disposal
- Any component that subscribes to events, timers, or holds unmanaged resources **MUST** implement `IAsyncDisposable`.
- **ALWAYS** unsubscribe from all events in `DisposeAsync()`:
  ```csharp
  // ✅ CORRECT
  public async ValueTask DisposeAsync()
  {
      _someService.OnDataChanged -= HandleDataChanged;
      await _someResource.DisposeAsync();
  }
  ```

### Callbacks
- **PREFER** `EventCallback<T>` over `Action<T>` or `Func<T, Task>` for component event parameters.
- `EventCallback<T>` is struct-based, automatically marshals to the correct thread, and does not hold strong references that prevent garbage collection.
  ```csharp
  // ✅ CORRECT
  [Parameter] public EventCallback<Product> OnSelected { get; set; }

  // ❌ WRONG — Action holds a strong reference, bypasses Blazor's render pipeline
  [Parameter] public Action<Product>? OnSelected { get; set; }
  ```

### Cancellation Tokens
- **ALWAYS** accept and forward `CancellationToken` in service methods that perform async I/O.
- Pass `CancellationToken` to every EF Core call (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, etc.):
  ```csharp
  // ✅ CORRECT
  public async Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
  {
      await using var context = await _factory.CreateDbContextAsync(cancellationToken);
      return await context.Products.AsNoTracking().ToListAsync(cancellationToken);
  }
  ```
