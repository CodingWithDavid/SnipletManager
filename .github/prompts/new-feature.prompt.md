---
model: gpt-4o
description: Builds a new Blazor feature with security and style checks.
---

@blazor-architect

I need a new feature: {{input}}

Please execute the following:
1. **Design**: Briefly describe the UI based on `.github/instructions/ui-style-guide.md`. Use CSS variables only — no hardcoded hex values in `.razor.css` files.
2. **Build**: Generate the Razor pages and C# logic following `.github/instructions/coding-standards.md`, including:
   - Section 6 resource management: `await using` DbContext, `IAsyncDisposable` on components that subscribe to events, `CancellationToken` on all async DB calls, `EventCallback<T>` over `Action<T>`.
   - All configurable values via `IOptions<T>` — see `.github/instructions/configuration-standards.md`. No `const` or inline literals for configurable values.
3. **Audit**: Run both `blazor-audit` and `blazor-security` skills on all generated code and fix any issues found *before* outputting the final result.
