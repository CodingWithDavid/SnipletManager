# Security & Quality Protocols

> ⚠️ This file is the **single source of truth** for all security checks.
> Both `blazor-security` and `legacy-aware-audit` skills delegate to this checklist.
> Update only here — never duplicate this list in skill or agent files.

---

## Security Audit Checklist

Run every item below against any generated or modified code. Mark each as PASS or FAIL.

### 1. Authorization
- [ ] Does every page have `@attribute [Authorize]` unless explicitly marked public?
- [ ] Are role/policy checks (`[Authorize(Roles = "...")]`) applied where appropriate?
- **SEVERITY**: Critical if missing

### 2. Input Validation
- [ ] Are all user inputs validated server-side?
- [ ] For raw HTML forms: Is manual validation implemented in the `@code` block?
- [ ] Are `required`, `maxlength`, and type constraints set on `<input>` elements?
- [ ] Are model properties annotated with data annotations (e.g., `[Required]`, `[StringLength]`)?
- **NOTE**: Do NOT check for `<DataAnnotationsValidator>` — we do not use `<EditForm>`.
- **SEVERITY**: High if missing

### 3. Anti-Forgery (CSRF)
- [ ] Does every state-modifying raw HTML `<form>` include `<AntiforgeryToken />`?
- [ ] Are POST handlers protected against cross-site request forgery?
- **SEVERITY**: High if missing

### 4. XSS Prevention
- [ ] Is `MarkupString` used with user-generated content? (Forbidden — flag immediately)
- [ ] Are all user inputs rendered via standard Razor binding (`@bind`, `@value`) not string interpolation?
- **SEVERITY**: Critical if vulnerable

### 5. Data Exposure
- [ ] Are sensitive entity properties (Password, Token, Secret) exposed to the UI?
- [ ] Are DTOs used to transfer data to Razor components instead of raw EF entities?
- **SEVERITY**: Critical if sensitive data is exposed

### 6. SQL Injection Prevention
- [ ] Are raw SQL strings used (e.g., `FromSqlRaw` with string interpolation)?
- [ ] Is Entity Framework Core with parameterized queries or `.FromSqlRaw` with explicit parameters used?
- **SEVERITY**: Critical if vulnerable

### 7. Secrets & Configuration
- [ ] Are any secrets, API keys, or passwords hardcoded in source code?
- [ ] Are sensitive values stored in `appsettings.json` user secrets (dev) or environment variables / Azure Key Vault (prod)?
- **SEVERITY**: Critical if hardcoded

### 8. Error Handling
- [ ] Are all service calls wrapped in `try-catch`?
- [ ] Are errors logged with `ILogger` without exposing internal details to the user?
- **SEVERITY**: Medium if missing

### 9. Rate Limiting
- [ ] Are authentication endpoints (login, register, password reset) protected by ASP.NET Core rate limiting middleware?
- [ ] Is `builder.Services.AddRateLimiter(...)` configured in `Program.cs` with a fixed-window or sliding-window policy?
- [ ] Are rate-limited endpoints decorated with `[EnableRateLimiting("policy-name")]`?
- **HOW TO FIX**: Add the following to `Program.cs`:
  ```csharp
  builder.Services.AddRateLimiter(options =>
  {
      options.AddFixedWindowLimiter("auth", policy =>
      {
          policy.PermitLimit = 5;
          policy.Window = TimeSpan.FromMinutes(1);
          policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
          policy.QueueLimit = 0;
      });
  });
  app.UseRateLimiter();
  ```
  Permit limit and window values must come from `appsettings.json` via `IOptions<T>`, not hardcoded.
- **SEVERITY**: High if missing on auth endpoints

### 10. Content Security Policy (CSP)
- [ ] Is a Content-Security-Policy HTTP response header configured in `Program.cs`?
- [ ] Does the policy restrict `script-src`, `style-src`, and `frame-ancestors` at minimum?
- **HOW TO FIX**: Add middleware in `Program.cs`:
  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Append(
          "Content-Security-Policy",
          "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; frame-ancestors 'none';");
      await next();
  });
  ```
  CSP policy string must be stored in `appsettings.json`, not hardcoded inline.
- **SEVERITY**: Medium if missing; High if app handles sensitive data

---

## Output Format

If issues are found, output this table:

| Severity | Rule # | Issue | Location | Required Fix |
|----------|--------|-------|----------|--------------|
| Critical | #1     | Missing [Authorize] | `UserProfile.razor:1` | Add `@attribute [Authorize]` |
| High     | #3     | No AntiforgeryToken | `EditUser.razor:12`  | Add `<AntiforgeryToken />` inside `<form>` |
| High     | #9     | No rate limiting on login | `Program.cs`  | Add `AddRateLimiter` with auth policy |

If no issues are found:
```
✅ Security Audit: PASSED — all 10 checks passed, no vulnerabilities detected.
```

---

## Code Quality Gates

1. **Null Safety**: All parameters must check for nulls or use C# nullable types (`<Nullable>enable</Nullable>`).
2. **Error Handling**: Wrap all service calls in a `try-catch` block and log the error to `ILogger`.
3. **No hardcoded values**: Timeouts, limits, feature flags, and endpoints must come from `IOptions<T>` / `appsettings.json`.
