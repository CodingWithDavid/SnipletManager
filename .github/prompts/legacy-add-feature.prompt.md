---
model: gpt-4o
description: Adds a new feature to an existing legacy Blazor application using greenfield standards.
---

@legacy-feature-builder

I need to add a new feature to an existing legacy application: {{input}}

**Context:**
- This is a legacy Blazor Server app (.NET 10)
- The new feature should follow greenfield standards (raw HTML, QuickGrid, isolated CSS)
- Integration must respect existing architecture and patterns

**Your Task:**
1. **Analyze First**: Examine the existing codebase to understand:
   - Folder structure and file organization
   - Service registration patterns in `Program.cs`
   - Data access patterns (DbContext usage, repositories)
   - Routing conventions
   - Naming conventions

2. **Design**: Plan the new feature using our greenfield standards from `.github/instructions/`:
   - Raw HTML forms (`<form>`, `<input>`, `<button>`)
   - `<QuickGrid>` for data lists
   - Isolated CSS files (`.razor.css`)
   - Clean Architecture (services, interfaces, DTOs)

3. **Build**: Implement the feature following `.github/instructions/coding-standards.md`:
   - Create Razor components with `@rendermode InteractiveServer`
   - Create required services and interfaces
   - Use `IDbContextFactory<T>` with `await using` for database access (Section 6)
   - Implement `IAsyncDisposable` on any component that subscribes to events (Section 6)
   - Pass `CancellationToken` to all async DB calls (Section 6)
   - Use `EventCallback<T>` over `Action<T>` for component callbacks (Section 6)
   - Use `IOptions<T>` for all configurable values — see `.github/instructions/configuration-standards.md`
   - Use CSS variables only in `.razor.css` files — no hardcoded hex values
   - Apply proper error handling with `ILogger`

4. **Integrate**: Connect with existing patterns:
   - Place files in folders matching existing conventions
   - Register services following existing DI patterns
   - Update routing if needed
   - Connect to existing `DbContext` or data access layer

5. **Audit**: Run the following skills on all generated code:
   - `blazor-audit` skill (component rules + resource management checks)
   - `blazor-security` skill
   - `security-audit` skill
   - Fix any issues found before providing final output

**Output:**
Provide complete implementation including:
- All new `.razor` files with isolated `.razor.css` files
- Service interfaces and implementations
- Code snippet for service registration in `Program.cs`
- Any required navigation/routing updates
- Summary of integration points with existing code
