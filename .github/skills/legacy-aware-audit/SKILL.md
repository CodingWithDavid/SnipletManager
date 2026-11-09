---
name: legacy-aware-audit
description: Audits legacy code changes to verify style preservation and security compliance.
disable-model-invocation: false
---

# Legacy-Aware Audit Skill

Use this skill to audit code changes in legacy projects, ensuring both security compliance and style preservation.

## Purpose
This skill performs a dual audit:
1. **Security Audit**: Checks for security vulnerabilities (same as `blazor-security`)
2. **Style Preservation Audit**: Verifies that refactors/bug fixes maintain existing patterns

---

## Audit Checklist

### Part 1: Security Audit (Always Required)

**Do not use an inline checklist here.** Instead:

1. Read `.github/instructions/security-protocols.md` — this is the single source of truth for all security checks.
2. Run every item in the **Security Audit Checklist** (Rules #1–#10) against the modified code.
3. Use the **Output Format** defined in that file to report security findings.

---

### Part 2: Style Preservation Audit (For Refactors & Bug Fixes Only)

**Skip this section if**: Adding a new feature (new code should use greenfield standards)

**Check these areas** if refactoring or fixing bugs in existing code:

1. **Component Pattern Consistency**
   - [ ] **FAIL** if `<EditForm>` was changed to raw HTML `<form>`
   - [ ] **FAIL** if `<InputText>` was changed to `<input>`
   - [ ] **FAIL** if raw HTML was changed to Blazor components
   - [ ] **PASS** if component types remain unchanged

2. **CSS Approach Consistency**
   - [ ] **FAIL** if isolated CSS was moved inline
   - [ ] **FAIL** if inline styles were extracted to isolated CSS
   - [ ] **FAIL** if CSS class names were changed
   - [ ] **PASS** if CSS approach remains unchanged

3. **Route Path Consistency**
   - [ ] **FAIL** if `@page` path was changed (unless explicitly requested)
   - [ ] **PASS** if route paths remain unchanged

4. **Method Signature Consistency** (for public/protected methods)
   - [ ] **FAIL** if method parameters changed (breaking change)
   - [ ] **FAIL** if return type changed (breaking change)
   - [ ] **PASS** if signatures remain compatible

5. **Naming Convention Consistency**
   - [ ] **FAIL** if naming style changed (PascalCase ↔ camelCase)
   - [ ] **FAIL** if suffixes changed (Service → Manager)
   - [ ] **PASS** if naming matches surrounding code

---

## Audit Context Detection

Before auditing, determine the change type:

| Change Type | Security Audit | Style Preservation Audit |
|-------------|---------------|-------------------------|
| **New Feature** | ✅ Required | ❌ Skip (new code uses greenfield standards) |
| **Refactor** | ✅ Required | ✅ Required |
| **Bug Fix** | ✅ Required | ✅ Required |

**How to detect**:
- If files are newly created → New Feature
- If existing files modified without bug context → Refactor
- If fixing specific error/issue → Bug Fix

---

## Output Format

### If Issues Found

Provide a table with all issues:

| Category | Severity | Issue | Location | Required Fix |
|----------|----------|-------|----------|--------------|
| Security | Critical | Missing [Authorize] | UserProfile.razor:1 | Add `@attribute [Authorize]` |
| Security | High | No input validation | LoginForm.razor:45 | Add DataAnnotationsValidator |
| Style | High | EditForm → raw HTML | OrderEdit.razor:12 | Revert to EditForm |
| Style | Medium | Route path changed | Products.razor:1 | Restore original path |

**Then**: Stop and explain that these issues MUST be fixed before proceeding.

---

### If No Issues Found

Provide a summary:

```
✅ Security Audit: PASSED
- Authorization: Proper [Authorize] attributes present
- Input Validation: Properly implemented
- XSS Prevention: No vulnerabilities detected
- SQL Injection: Using EF Core safely
- Error Handling: Proper logging in place

✅ Style Preservation Audit: PASSED
- Component patterns unchanged (EditForm preserved)
- CSS approach consistent (isolated CSS maintained)
- Route paths unchanged
- Method signatures compatible
- Naming conventions consistent

Code is ready for deployment.
```

---

## Special Cases

### Case 1: Security Fix Required During Refactor
If you find a security issue during a refactor:
- **DO** fix the security issue (security overrides style preservation)
- **Document** it separately as a security fix
- **DO NOT** change other style elements

### Case 2: Breaking Change Necessary for Bug Fix
If fixing a bug requires a breaking change:
- **Flag** it in the audit
- **Require** user confirmation before proceeding
- **Document** the breaking change and impact

### Case 3: New Feature in Legacy Project
If auditing new feature code in a legacy project:
- **DO** check security (always required)
- **SKIP** style preservation (new code uses greenfield standards)
- **DO** verify integration points match existing conventions

---

## Integration with Other Skills

This skill works alongside:
- `blazor-security`: Focuses purely on security
- `blazor-audit`: Checks greenfield component rules
- `security-audit`: Checks OWASP compliance

**Use this skill specifically for**:
- Legacy refactoring work
- Legacy bug fixes
- Any change to existing legacy code

**Don't use this skill for**:
- Brand new greenfield projects (use `blazor-audit` instead)
- New features in legacy projects (security audit only, skip style check)

---

## Example Audit Scenarios

### Scenario 1: Refactoring User Service ✅ PASS
```
Change: Extracted business logic from UserProfile.razor to UserService
Security: [Authorize] present, no vulnerabilities
Style: Component pattern unchanged (still uses EditForm), CSS unchanged
Result: APPROVED
```

### Scenario 2: Bug Fix with Style Violation ❌ FAIL
```
Change: Fixed null reference bug by converting EditForm to raw HTML
Security: No issues
Style: FAIL - Component pattern changed (EditForm → raw HTML)
Result: REJECTED - Fix the bug without changing component pattern
```

### Scenario 3: New Feature in Legacy App ✅ PASS
```
Change: Added new Product Catalog page using raw HTML (greenfield standard)
Security: [Authorize] present, validation implemented, no vulnerabilities
Style: SKIP - New feature uses greenfield standards (correct behavior)
Result: APPROVED
```

---

## Quick Reference

**When to use this skill**:
- ✅ Refactoring existing legacy code
- ✅ Fixing bugs in legacy code
- ❌ Creating new features (use `blazor-security` only)
- ❌ Greenfield projects (use `blazor-audit` instead)

**What it checks**:
- Security compliance (always)
- Style preservation (only for refactors/bug fixes)

**Failure modes**:
- Critical security issue → STOP, fix immediately
- Style violation in refactor → STOP, preserve original style
- Style "violation" in new feature → IGNORE, new code uses greenfield standards
