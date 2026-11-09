---
name: advance-code-analysis
description: Run an advanced static code-analysis workflow for repositories when the user asks for deeper code-quality audits, especially finding duplicated business rules, errors discovered only after they occur, silently unexpected behavior, or high-level modules depending on low-level implementations. Use this skill when Codex should coordinate the four audits through subagents and save each audit as a separate Markdown document under a documents folder.
---

# Advance Code Analysis

## Overview

Run four focused static analysis passes in parallel or near-parallel, each delegated to its own subagent. Each subagent must inspect the repository independently and save exactly one Markdown report in a `documents/` folder.

## Workflow

1. Confirm the repository root from the current working directory or the user's supplied path.
2. Create `documents/` at the repository root if it does not already exist.
3. Launch one subagent for each audit prompt below. Give each subagent only the repository path, the audit prompt, and the required output path.
4. Require subagents to perform static analysis first. They may run build/test/search commands only when useful and safe for evidence.
5. After subagents finish, verify all four Markdown files exist and are non-empty.
6. Summarize the four outputs for the user with report paths and the highest-priority findings. Do not merge the reports unless the user asks.

## Required Reports

Use these filenames unless the user requests different names:

- `documents/business-rules-defined-more-than-once.md`
- `documents/errors-discovered-after-they-occur.md`
- `documents/unexpected-silent-behavior.md`
- `documents/high-level-depends-on-low-level.md`

## Subagent Prompts

Use one subagent per prompt.

### Duplicated Business Rules

Analyze the code for any place where a business rule is defined more than once. Look for repeated constants, duplicated conditionals, repeated validation logic, repeated authorization/status/payroll/timekeeping rules, or similar rule fragments implemented in multiple modules. Distinguish true duplicated business rules from harmless repeated syntax. Save the findings to `documents/business-rules-defined-more-than-once.md`.

### Errors Discovered After They Occur

Analyze the code for any place where errors are discovered only after they occur instead of being prevented, validated, or surfaced earlier. Look for delayed validation, catch-and-recover flows that could be prechecked, null/default fallbacks that defer failure, background processing that notices invalid state late, and workflows that rely on downstream exceptions for control flow. Save the findings to `documents/errors-discovered-after-they-occur.md`.

### Unexpected Silent Behavior

Analyze the code for any place where the code does unexpected things silently. Look for swallowed exceptions, default/null return fallbacks, hidden mutations, implicit cache refreshes, ignored failed operations, console-only logging in server code, missing user-visible errors, or behavior that changes state without a clear caller signal. Save the findings to `documents/unexpected-silent-behavior.md`.

### High-Level Depends On Low-Level

Analyze the code for any place where high-level modules depend on low-level implementations. Look for UI/pages/controllers depending directly on database contexts, concrete services, transport clients, filesystem/blob/email implementations, service locators, static service state, or concrete infrastructure classes where an application/domain abstraction would be expected. Save the findings to `documents/high-level-depends-on-low-level.md`.

## Report Format

Each Markdown report must use this shape:

```markdown
# <Audit Title>

## Scope
- Repository/path reviewed:
- Static analysis date:
- Commands or searches used:

## Findings
### 1. <Finding title>
- Severity: High | Medium | Low
- Evidence: <file path and line references>
- Why it matters:
- Suggested direction:

## Non-Issues / Intentional Patterns
- <Optional>

## Residual Risk
- <What was not verified or may need runtime validation>
```

Prefer concrete file and line references over broad architectural claims. If no findings are found for a prompt, the report must say so clearly and explain the searches performed.

## Coordination Notes

- Keep the four audits independent so repeated findings across reports reveal cross-cutting risk rather than shared assumptions.
- Instruct subagents not to edit application code.
- If subagents are unavailable, perform the four passes sequentially and still create the four Markdown reports.
- If the repo is very large, scope generated/vendor/build folders out of the search unless they are directly relevant.
