---
name: sundt-ui-components
description: 'Guides app-building agents to add and wire components from SundtUILibrary.Components.UI into a Blazor application. WHEN: "add navigation or feedback ui component", "use dialog or tooltip component", "scaffold navbar sidebar breadcrumb", "wire toast host integration".'
disable-model-invocation: false
---

# UI Component Skill

## What this skill does

Use this skill when an agent needs to add a component from `SundtUILibrary.Components.UI` into a host Blazor application and wire it correctly.

This skill helps with:

- Selecting a suitable component in this group.
- Generating integration-ready markup.
- Wiring parameters, callbacks, and defaults.
- Applying baseline accessibility and security checks.

## Component inventory

| Component | Source |
|---|---|
| `ColorSwatch` | `SundtUILibrary/Components/UI/ColorSwatch.razor` |
| `DtBreadcrumb` | `SundtUILibrary/Components/UI/DtBreadcrumb.razor` |
| `DtContextualNotice` | `SundtUILibrary/Components/UI/DtContextualNotice.razor` |
| `DtDialog` | `SundtUILibrary/Components/UI/DtDialog.razor` |
| `DtFeedbackWidget` | `SundtUILibrary/Components/UI/DtFeedbackWidget.razor` |
| `DtLogo` | `SundtUILibrary/Components/UI/DtLogo.razor` |
| `DtNavSidebar` | `SundtUILibrary/Components/UI/DtNavSidebar.razor` |
| `DtProgressPanel` | `SundtUILibrary/Components/UI/DtProgressPanel.razor` |
| `DtTooltip` | `SundtUILibrary/Components/UI/DtTooltip.razor` |
| `DtTopNavbar` | `SundtUILibrary/Components/UI/DtTopNavbar.razor` |
| `ToastHost` | `SundtUILibrary/Components/UI/ToastHost.razor` |

## Scaffold into an application

1. Add import(s) to host `_Imports.razor` or page-level imports:
   - `@using SundtUILibrary`
   - `@using SundtUILibrary.Components`
   - `@using SundtUILibrary.Components.UI`
2. Ensure static assets/styles for `SundtUILibrary` are available in the consuming app.
3. Service registration guidance:
   - Register ToastService in DI when using ToastHost or UI flows that emit toast notifications.
4. Add a starter markup block in the target page/component using one inventory component, for example `ColorSwatch`.
5. Scaffold host state and handlers:
   - Add local state properties for bound values.
   - Add `EventCallback`/handler methods for emitted events.
   - Supply safe defaults for optional parameters.
6. Validate behavior in three states:
   - Positive (expected data)
   - Negative (missing/invalid inputs)
   - Edge (empty collections, max/min values, long text)

### Minimal scaffold pattern

- Add imports.
- Add the selected component markup.
- Add state and callback handlers in `@code`.
- Run and verify render + interaction.

## Wiring checklist

- Required parameters provided.
- Event callbacks connected to host handlers.
- State initialized in the host component/page.
- `@bind-...` values use deterministic defaults.
- Labels, ARIA attributes, and semantic button text included.

## Security and safety checks

- Do not embed secrets in component parameters or markup.
- Treat host-provided content as untrusted; encode/sanitize where appropriate.
- Keep authorization and data-access enforcement in the host app.
- Validate user input in host flows before persistence or API calls.

## Common pitfalls

- Missing `@using SundtUILibrary.Components.UI` import.
- Callbacks not wired, causing non-interactive controls.
- Using placeholder data but not replacing it with host state.
- Omitting accessibility labels for interactive elements.

## Agent output format

When applying this skill, output:

1. Chosen component and why it fits the request.
2. Minimal host-app scaffold changes (imports/services/markup/state/handlers).
3. Parameter and callback wiring notes.
4. Quick verification checklist for the consuming app.

