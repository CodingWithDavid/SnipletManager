---
model: gpt-4o
description: Scaffolds a brand new .NET 10 Blazor Server application from scratch.
---

@plan

I want to create a new application. Here is what it should do: {{input}}

**Mandatory Application Requirements:**
1. **Framework**: Must use **.NET 10**.
2. **Hosting Model**: Must use **Blazor Server**. Apply `@rendermode InteractiveServer` globally.
3. **Database**: Implement SQLite with Entity Framework Core using `IDbContextFactory` as per `.github/instructions/database-standards.md`.
4. **Architecture**: Read `.github/instructions/coding-standards.md` — all business logic in scoped services; all configurable values (timeouts, page sizes, endpoints) via `IOptions<T>` per `.github/instructions/configuration-standards.md`.
5. **Styling**: Read `.github/instructions/ui-style-guide.md` — Isolated CSS with CSS variables only. No hardcoded hex values or pixel literals in component files.
6. **Resource Management**: Apply Section 6 of `.github/instructions/coding-standards.md` — `await using` DbContext, `IAsyncDisposable` on components that subscribe to events, `CancellationToken` on all async DB calls, `EventCallback<T>` over `Action<T>`.

**Your Task:**
Act as the Solution Architect. Do not write the final code yet. Instead, perform a deep analysis and generate a step-by-step implementation plan. Break down the task into manageable, actionable steps (e.g., scaffolding the project, setting up the database, creating the services, and building the UI). Ask me clarifying questions if any requirements are ambiguous.