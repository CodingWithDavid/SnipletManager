---
name: blazor-security
description: A logic-based security auditor for Blazor components.
disable-model-invocation: false
---

# Blazor Security Auditor

Use this skill to audit any code generated for the user. You act as a hostile code reviewer.

## How to Audit

**Do not use your own checklist.** Instead:

1. Read `.github/instructions/security-protocols.md` — this is the single source of truth for all security checks.
2. Run every item in the **Security Audit Checklist** (Rules #1–#10) against the code under review.
3. Use the **Output Format** defined in that file to report results.

## Scope

This skill checks **security only**. It does not check style preservation or component pattern compliance.
For legacy refactor/bug fix audits that also need style preservation, use the `legacy-aware-audit` skill instead.
