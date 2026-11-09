---
name: return-contract-audit
description: Audit a codebase for consistency in method return behavior on missing data and errors, including null, empty collections, empty/default objects, bool success flags, Result-style wrappers, thrown exceptions, and swallowed exceptions. Use when the user asks to analyze how consistently methods return on error, null, empty object, Results-style wrappers, or other return types, and to write the findings into the repo's documents folder.
---

# Return Contract Audit

Use this skill to perform a read-only consistency audit of return contracts across a repository and write the result to `documents/`.

## Workflow

1. Inspect the repository shape with `rg --files`, prioritizing source directories and excluding generated/static/vendor assets such as `bin`, `obj`, `node_modules`, `wwwroot`, `dist`, and `build`.
2. Identify public contracts first: interfaces, services, repositories, controllers/endpoints, view models, and mappers.
3. Search for return mechanisms:
   - `return null;`, `return default;`
   - `return []`, `new List<T>()`, `Array.Empty<T>()`, `Enumerable.Empty<T>()`
   - empty/default object returns such as `return new Foo()` or `return new()`
   - `return true;` / `return false;` from command methods
   - `Result<T>`, `Results<T>`, `IResult`, `ActionResult`, `ValidationResult`, `ServiceResult`, `OperationResult`, `ErrorOr`, `OneOf`
   - `catch`, `throw`, `throw new`, and log-only catch blocks
4. Separate valid no-data behavior from operational failure behavior. Call out any method where one return value can mean more than one thing.
5. Write a Markdown report into `documents/` with:
   - overall consistency assessment
   - counts by pattern
   - key examples with file and line references
   - module-by-module observations
   - risks and recommended conventions
   - suggested first cleanup target

## Helper Script

Run the bundled script from the repository root when a quick first-pass report is useful:

```powershell
python C:\Users\dhgallivan\.codex\skills\return-contract-audit\scripts\return_contract_audit.py --repo . --out documents
```

After the script runs, read the generated report and refine it with manual code inspection before finalizing. The script is a starting point, not a replacement for judgment.

## Reporting Guidance

Prefer precise, behavior-oriented language:

- "This method returns an empty list on SQL failure, so callers cannot distinguish failure from no rows."
- "This service returns `false` for both not found and caught exception."
- "This interface declares a non-null DTO, but the implementation returns an empty shell object when data is missing."

Avoid recommending a repo-wide `Result<T>` conversion unless the blast radius is justified. A practical recommendation is usually:

- collections: empty only for valid no-data, not operational failure
- single item lookups: `T?` for not found
- commands: `Result` / `Result<T>` when callers need a reason, `bool` only for pure predicates
- infrastructure failures: preserve the error by throwing or returning a typed failure
