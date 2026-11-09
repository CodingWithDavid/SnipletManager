---
name: sundt-ui-layout-components
description: 'Guides app-building agents to add and wire components from SundtUILibrary.Components.UI.Layout into a Blazor application. WHEN: "add layout component", "use carousel card gallery flip card", "scaffold layout composition", "wire layout item interaction callbacks".'
disable-model-invocation: false
---

# Layout Component Skill

## What this skill does

Use this skill when an agent needs to add a component from `SundtUILibrary.Components.UI.Layout` into a host Blazor application and wire it correctly.

This skill helps with:

- Selecting a suitable component in this group.
- Generating integration-ready markup.
- Wiring parameters, callbacks, and defaults.
- Applying baseline accessibility and security checks.

## Component inventory

| Component | Source |
|---|---|
| `DtCardGallery` | `SundtUILibrary/Components/UI/Layout/DtCardGallery.razor` |
| `DtCarousel` | `SundtUILibrary/Components/UI/Layout/DtCarousel.razor` |
| `DtFlipCard` | `SundtUILibrary/Components/UI/Layout/DtFlipCard.razor` |

## Scaffold into an application

1. Add import(s) to host `_Imports.razor` or page-level imports:
   - `@using SundtUILibrary`
   - `@using SundtUILibrary.Components`
   - `@using SundtUILibrary.Components.UI.Layout`
2. Ensure static assets/styles for `SundtUILibrary` are available in the consuming app.
3. Service registration guidance:
   - No additional service registration is required for most components in this group; register services only when a selected component explicitly needs one.
4. Add a starter markup block in the target page/component using one inventory component, for example `DtCardGallery`.
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

- Missing `@using SundtUILibrary.Components.UI.Layout` import.
- Callbacks not wired, causing non-interactive controls.
- Using placeholder data but not replacing it with host state.
- Omitting accessibility labels for interactive elements.

## Agent output format

When applying this skill, output:

1. Chosen component and why it fits the request.
2. Minimal host-app scaffold changes (imports/services/markup/state/handlers).
3. Parameter and callback wiring notes.
4. Quick verification checklist for the consuming app.

