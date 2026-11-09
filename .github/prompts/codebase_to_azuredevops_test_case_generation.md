# Codebase → Azure DevOps Test Plan & Test Case Generation Prompt

## Role / Goal

You are a senior QA engineer and test architect. Analyze this repository
(open in VS Code) and produce a comprehensive set of **test plans and
test cases** based on what the system actually does. Output the test
cases in a **CSV formatted exactly for Azure DevOps import** using the
columns and row structure described below.

------------------------------------------------------------------------

## What to Analyze in the Codebase (VS Code Repo Scan)

1.  Identify the product's main functional areas by reading:
    -   Routes/pages/screens (UI)
    -   Controllers/endpoints/services (API)
    -   Business logic and validation rules
    -   Authorization/roles/permissions checks
    -   Data access/repositories/entities/migrations
    -   Feature flags/config
    -   Error handling/logging
2.  Extract testable behaviors:
    -   CRUD flows
    -   Workflow/state transitions
    -   Required fields and validation
    -   Permissions/role-based visibility and access
    -   Edge cases (empty/large inputs, boundary values, missing
        dependencies)
    -   Concurrency or "double-submit" scenarios
    -   Import/export/report flows if present
3.  Build test coverage:
    -   Happy path
    -   Negative/validation
    -   Security/authorization
    -   Regression/smoke candidates
    -   High-risk areas (money, permissions, state changes)

------------------------------------------------------------------------

## Output Requirements (VERY IMPORTANT)

### A) Create Test Plan Structure (Text Only)

Before the CSV, provide a short **Test Plan Outline** (plain text /
markdown) that groups test cases into logical suites, such as:

-   Authentication & Session
-   Authorization & Roles
-   Feature Area A
-   Feature Area B
-   Reporting
-   Error Handling / Observability
-   Data Integrity & Concurrency

This outline is for human readability only. The importable artifact is
the CSV.

------------------------------------------------------------------------

### B) Produce Azure DevOps Import CSV (Exact Shape)

You MUST output a single CSV with **exactly these columns in this
order**:

Work Item Type,Title,Test Step,Step Action,Step Expected,State

------------------------------------------------------------------------

### Row Structure Rules (Match Azure DevOps Format)

-   Each **new test case** begins with a header row:
    -   Work Item Type = Test Case
    -   Title = short, specific, unique
    -   State = Open
    -   Test Step, Step Action, Step Expected = blank on this header row
-   Immediately after the header row, include one row per test step:
    -   Work Item Type and Title must be blank for step rows
    -   Test Step is 1, 2, 3, etc. (numeric)
    -   Step Action describes the action
    -   Step Expected describes the expected result
    -   State is blank on step rows

------------------------------------------------------------------------

### CSV Formatting Rules

-   Output CSV only (no markdown tables) under a section header like:

    ### CSV

-   Escape quotes properly (double quotes inside fields).

-   If a field contains commas or line breaks, wrap it in quotes.

-   Keep step actions and expected results concise but unambiguous.

------------------------------------------------------------------------

## Coverage Expectations

Generate at minimum:

-   10--20 smoke/regression test cases across core flows
-   3--6 test cases per major feature area identified
-   Dedicated permission/role tests wherever roles exist
-   Validation negatives for each important form/input

Prefer smaller, focused test cases over large multi-scenario cases.

------------------------------------------------------------------------

## Naming Convention for Titles

Use a consistent format:

```{=html}
<Area>
```
-   `<Scenario>`{=html} - `<Expected Outcome>`{=html}

Example:

Projects - Close Project - Button visible for Accounting role

------------------------------------------------------------------------

## Assumptions Handling

-   If behavior cannot be confirmed from code, append
    `(Needs Confirmation)` to the test title.
-   Do NOT invent endpoints, UI, or workflows not supported by the
    repository.

------------------------------------------------------------------------

## Execution Instructions

1.  Scan the repository.
2.  Produce:
    -   Test Plan Outline (grouped list)
    -   Azure DevOps--importable CSV as specified

------------------------------------------------------------------------

## Recommended Two-Pass Strategy (Optional)

If the repository is large:

1.  Inventory pass:
    -   List features, roles, pages, endpoints, and file paths.
2.  Test generation pass:
    -   Use the inventory to generate the structured CSV test cases.
