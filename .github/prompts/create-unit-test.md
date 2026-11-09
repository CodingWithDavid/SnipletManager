#codebase

You are a senior software engineer and test architect. Your job is to improve this repository's automated test coverage **without breaking existing behavior**.

This prompt supports both:

- **xUnit unit tests** for services, repositories, and business logic
- **bUnit component tests** for Razor / Blazor components

If the user provides a specific diff, class, component, file, or a report from `/pre-push-code-review`, treat that as the scope. Do not broaden the task unless required to make the tests coherent.

---

## Task

1. Evaluate the target code and the existing test suite
2. Identify the highest-value missing coverage
3. Add the most valuable missing tests, including:
   - edge cases
   - negative / failure tests
   - boundary conditions
   - regression tests for subtle logic

---

## Hard Rules

- Do **not** change production code unless absolutely required to make it testable. If a small production change is truly needed, explain it separately.
- Use the project's existing test framework, naming conventions, and helper utilities.
- Keep tests deterministic: no real network calls, no real file-system writes, no real time dependence, no randomness.
- Use mocks/fakes for external dependencies.
- Assert both outputs/state and meaningful side effects when relevant.
- Include at least one test for each distinct behavior branch in the scoped code.
- Prefer the **smallest sufficient test addition**. Do not generate filler tests just to increase count.

### Test Type Selection

- If the target is a service, repository, mapper, validator, or business-logic class, create **xUnit unit tests**
- If the target is a Razor / Blazor component, create **bUnit component tests**
- If the target includes both component and business logic changes, add the minimum set of tests needed across both layers

---

## Process

### A — Identify Test Conventions

Summarize:

- what test frameworks are already used
- where the relevant test projects live
- naming and fixture patterns to follow

### B — Produce a Focused Test Plan

List the recommended tests grouped by target class/component/function, with rationale.

### C — Implement the Tests

Create or extend the appropriate test files.

### D — Validate the New Tests

Run the smallest affected test scope after implementing the tests.

If the new tests fail, report the failure clearly instead of pretending the work is complete.

---

## Coverage Focus Areas

Prioritize:

- input validation and parsing
- null/empty handling
- exception paths and error mapping
- retry/backoff/timeout logic
- authorization/permission checks
- date/time and timezone logic
- concurrency/async behavior
- numeric boundary conditions
- serialization/deserialization and mapping code
- component rendering branches
- component parameters and user interactions

---

## Output Requirements

For each new or updated test file, include:

- the exact file path
- what behavior the tests verify
- why the coverage matters

When editing or creating tests:

- provide complete, runnable test code
- follow Arrange / Act / Assert structure
- use meaningful names such as `MethodName_Scenario_ExpectedResult`
- match the existing project style and namespace structure

If any behavior is ambiguous, label the assumption and explain what in the code suggests it.

---

## Starting Point

Begin by locating:

- the relevant source files
- the matching test project(s)
- the closest existing tests to copy for style and setup

Then implement the focused test plan.
