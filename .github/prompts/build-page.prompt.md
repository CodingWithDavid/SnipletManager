---
model: gpt-4o
description: Scaffolds a Blazor Server page using raw HTML and QuickGrid.
---

@architect

I need a new page for: {{input}}

**Requirements:**
1. **Service-First**: Define the service interface needed to drive this page.
2. **Raw HTML**: Use standard HTML5 inputs for any forms.
3. **Data**: Use QuickGrid if displaying lists.
4. **Style**: Follow `.github/instructions/ui-style-guide.md` — use CSS variables only, no hardcoded hex values.
5. **Resource Management**: Apply Section 6 of `.github/instructions/coding-standards.md` — `await using` DbContext, `IAsyncDisposable` on components that subscribe to events, `CancellationToken` on all async DB calls.
6. **Configuration**: All configurable values (page sizes, timeouts) via `IOptions<T>` — see `.github/instructions/configuration-standards.md`.

**Audit**: Run `blazor-audit` and `blazor-security` skills on all generated code and fix any issues before outputting.

Please build the solution now.