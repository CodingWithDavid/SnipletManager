# Component Skills Contract

This contract defines folder-scoped component skills for `SundtUILibrary`.

## Skill Scope

One skill per component folder group:

1. `sundt-primitives-components` -> `SundtUILibrary.Components.Primitives`
2. `sundt-ui-components` -> `SundtUILibrary.Components.UI`
3. `sundt-ui-charts-components` -> `SundtUILibrary.Components.UI.Charts`
4. `sundt-ui-data-components` -> `SundtUILibrary.Components.UI.Data`
5. `sundt-ui-layout-components` -> `SundtUILibrary.Components.UI.Layout`

## Skill Location

All generated skills are written under `.github/skills/<skill-name>/SKILL.md`.

## Frontmatter Contract

Each generated skill must include:

- `name`: skill folder slug
- `description`: concise purpose plus `WHEN:` trigger phrases
- `disable-model-invocation: false`

## Required Sections

Every generated `SKILL.md` must contain:

1. `# <Group> Component Skill`
2. `## What this skill does`
3. `## Component inventory`
4. `## Scaffold into an application`
5. `## Wiring checklist`
6. `## Security and safety checks`
7. `## Common pitfalls`

## Scaffolding Requirements

Scaffolding instructions must tell an app-building agent to:

- Add the appropriate `@using SundtUILibrary.Components.*` import(s).
- Add required services if needed (for example `ToastService` when `ToastHost` is used).
- Add page/component markup for one or more components in the group.
- Bind callbacks/parameters with safe defaults.
- Validate visual behavior and accessibility labels.

## Discovery Source of Truth

Component groups are discovered from the namespace roots used by:

- `SundtUILibrary.Tests/Tests/Components/Primitives/AllPrimitivesComponentRenderingTests.cs`
- `SundtUILibrary.Tests/Tests/Components/UI/AllUiComponentRenderingTests.cs`
- `SundtUILibrary.Tests/Tests/Components/UI/Charts/AllChartComponentRenderingTests.cs`
- `SundtUILibrary.Tests/Tests/Components/UI/Data/AllDataComponentRenderingTests.cs`
- `SundtUILibrary.Tests/Tests/Components/UI/Layout/AllLayoutComponentRenderingTests.cs`

## Generation Rule

Skills are generated and overwritten by script. Manual edits to generated `SKILL.md` files are not preserved after regeneration.
