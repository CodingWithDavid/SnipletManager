# Configuration Standards

> All configurable values belong in `appsettings.json`, not in C# source code.
> This file defines the patterns all agents and developers must follow.

---

## 1. File Structure

Use environment-specific overrides. Values in the environment file win over the base file.

```
appsettings.json                  ← Base config, committed to source control (no secrets)
appsettings.Development.json      ← Dev overrides, committed (no secrets)
appsettings.Production.json       ← Prod overrides, committed (no secrets, use Key Vault refs)
```

**Never commit secrets to any `appsettings.*.json` file.**

---

## 2. Defining Settings — Options Pattern (`IOptions<T>`)

Every group of related settings gets a dedicated POCO class and a corresponding section in `appsettings.json`.

### Step 1 — Define the POCO in C#:
```csharp
// Settings/ProductSettings.cs
public class ProductSettings
{
    public const string SectionName = "ProductSettings";

    public int DefaultPageSize { get; init; } = 20;
    public int MaxPageSize    { get; init; } = 100;
    public int CacheDurationSeconds { get; init; } = 300;
}
```

### Step 2 — Add section to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "ProductSettings": {
    "DefaultPageSize": 20,
    "MaxPageSize": 100,
    "CacheDurationSeconds": 300
  }
}
```

### Step 3 — Register in `Program.cs`:
```csharp
builder.Services.Configure<ProductSettings>(
    builder.Configuration.GetSection(ProductSettings.SectionName));
```

### Step 4 — Inject in services:
```csharp
// ✅ CORRECT — value comes from config
public class ProductService(IOptions<ProductSettings> options)
{
    private readonly ProductSettings _settings = options.Value;

    public async Task<List<Product>> GetPageAsync(CancellationToken ct = default)
    {
        // Uses config value, not a hardcoded literal
        var pageSize = _settings.DefaultPageSize;
        ...
    }
}

// ❌ WRONG — magic number baked into source code
private const int PageSize = 20;
```

---

## 3. What Must Always Live in Config (Never in Source)

| Value Type | Example | Config Key Pattern |
|------------|---------|-------------------|
| Connection strings | SQLite path, SQL Server conn str | `ConnectionStrings:DefaultConnection` |
| Page / batch sizes | Grid page size, export limits | `FeatureSettings:DefaultPageSize` |
| Timeouts | HTTP client timeout, DB command timeout | `HttpSettings:TimeoutSeconds` |
| Feature flags | Enable/disable features per environment | `FeatureFlags:EnableReporting` |
| External endpoints | API base URLs | `ExternalApi:BaseUrl` |
| Rate limit thresholds | Max requests, window size | `RateLimiting:Auth:PermitLimit` |
| CSP policy string | Content-Security-Policy header value | `Security:CspPolicy` |

---

## 4. Secrets Management

### Development
Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) — values are stored outside the project folder, never in source control.

```bash
dotnet user-secrets init
dotnet user-secrets set "ExternalApi:ApiKey" "dev-key-here"
```

Access in code exactly like any other config key — no code changes needed between dev and prod.

### Production
Use **Azure Key Vault** references. Add the Key Vault provider in `Program.cs`:

```csharp
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = builder.Configuration["KeyVault:Uri"]
        ?? throw new InvalidOperationException("KeyVault:Uri is not configured.");

    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}
```

Key Vault secret names map to `appsettings.json` keys using `--` as the separator:
- Key Vault secret `ExternalApi--ApiKey` → `Configuration["ExternalApi:ApiKey"]`

---

## 5. Rules for Agents & Code Generation

When generating any code that involves a configurable value, enforce these rules:

- ❌ **NEVER** generate `private const string Url = "https://..."` 
- ❌ **NEVER** generate `private const int Timeout = 30`
- ❌ **NEVER** generate `private static readonly string Key = "abc123"`
- ✅ **ALWAYS** create a settings POCO, register it with `Configure<T>`, and inject `IOptions<T>`
- ✅ **ALWAYS** add the corresponding section to `appsettings.json` with safe default values
- ✅ **ALWAYS** add a comment in `appsettings.Production.json` noting that secrets must come from Key Vault

---

## 6. Environment-Specific Overrides Example

```json
// appsettings.json (base — committed)
{
  "ProductSettings": {
    "DefaultPageSize": 20,
    "CacheDurationSeconds": 300
  }
}

// appsettings.Development.json (committed — no secrets)
{
  "ProductSettings": {
    "CacheDurationSeconds": 0
  }
}

// appsettings.Production.json (committed — no secrets, secrets come from Key Vault)
{
  "ProductSettings": {
    "CacheDurationSeconds": 600
  }
  // ExternalApi:ApiKey → provided by Azure Key Vault at runtime
}
```
