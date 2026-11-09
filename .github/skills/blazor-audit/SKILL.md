---
name: blazor-audit
description: Reviews Blazor code to ensure it meets strict component usage rules.
---

# Blazor Audit Skill

Act as a strict code reviewer. Check the generated code against these Rules of Engagement:

## 1. The "Raw HTML" Check
- [ ] **FAIL** if you see `<EditForm>`, `<InputText>`, `<InputNumber>`, or `<InputDate>`.
- [ ] **PASS** if the code uses `<form>`, `<input>`, and `<select>` with `@bind`.

## 2. The "QuickGrid" Check
- [ ] **FAIL** if the code uses a `<table>` or `foreach` loop to render a data list.
- [ ] **PASS** if the code uses `<QuickGrid Items="...">`.

## 3. Security Check
- [ ] **Auth**: Is `@attribute [Authorize]` present at the top of the file?
- [ ] **Injection**: Are we passing raw strings to `MarkupString`? (Strictly Forbidden).

## 4. Resource Management Check (Memory Leak Prevention)

### DbContext Field Check
- [ ] **FAIL** if a `DbContext` (e.g., `AppDbContext`, any class ending in `Context`) is stored as a component field (`private AppDbContext _context`).
- [ ] **FAIL** if `IDbContextFactory` is called outside of an `await using` block.
- [ ] **PASS** if every context is created with `await using var context = await _factory.CreateDbContextAsync(...)` inside a method body.

### Event Subscription Check
- [ ] **FAIL** if the component subscribes to any event (e.g., `someService.OnChange += Handler`) but does NOT implement `IAsyncDisposable` or `IDisposable`.
- [ ] **FAIL** if `DisposeAsync()` or `Dispose()` exists but does not unsubscribe the same events that were subscribed in `OnInitializedAsync()` or `OnInitialized()`.
- [ ] **PASS** if every `+=` subscription has a corresponding `-=` in the dispose method.

### CancellationToken Check
- [ ] **FAIL** if any `async Task` service method performs EF Core calls (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, etc.) without accepting or forwarding a `CancellationToken`.
- [ ] **FAIL** if `CancellationToken` is accepted as a parameter but not passed through to database calls.
- [ ] **PASS** if every async DB call receives a `CancellationToken`.

## Output
If any check fails, stop and rewrite the code to fix it. Do not ask for permission, just fix it.
Report resource management failures with label **[MEMORY LEAK RISK]** so they are clearly distinguishable from style failures.