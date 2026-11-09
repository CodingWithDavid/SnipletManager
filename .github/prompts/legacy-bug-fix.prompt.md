---
model: gpt-4o
description: Fixes bugs in legacy code with minimal surgical changes and mandatory security enforcement.
---

@legacy-bug-fix

**Bug to fix**: {{input}}

**Critical Rules:**
- **Minimal changes**: Change ONLY what's necessary to fix the bug
- **Match existing style**: Use exact same code style, patterns, and conventions
- **Security is mandatory**: Run security audits and fix any issues found
- **No refactoring**: Don't improve unrelated code

**Your Task:**

1. **Locate and Understand**:
   - Read the bug description carefully
   - Locate the exact file and method causing the issue
   - Understand expected vs actual behavior
   - Trace data flow to identify root cause

2. **Plan Minimal Fix**:
   - Identify smallest change that resolves the bug
   - Consider edge cases the fix might affect
   - Plan for proper error handling if needed
   - Note any security implications

3. **Implement Surgical Fix**:
   - Make MINIMUM code change required
   - Match surrounding code style EXACTLY:
     - Same indentation and spacing
     - Same naming conventions
     - Same patterns (if they use `var`, you use `var`)
     - Same component patterns (EditForm stays EditForm)
   - Add necessary null checks or validation
   - Add error logging if appropriate

4. **Mandatory Security Audit** (Even for Simple Bugs):
   - Run `blazor-security` skill on the entire modified file
   - Run `security-audit` skill
   - **FIX all issues the skills report** — they cover the full checklist (authorization, CSRF, XSS, SQL injection, secrets, rate limiting, CSP). Do not skip any finding.

5. **Verify No Side Effects**:
   - Check if fix affects other parts of the code
   - Verify fix doesn't break existing behavior
   - Confirm the bug is actually resolved

**What NOT to Change:**
- ❌ Don't refactor working code around the bug
- ❌ Don't "improve" unrelated code
- ❌ Don't change coding style to match standards
- ❌ Don't add features while fixing bugs
- ❌ Don't change component patterns (EditForm → raw HTML)

**What TO Change:**
- ✅ The specific bug
- ✅ Security issues in the same file (MANDATORY)
- ✅ Null safety for the fixed code path
- ✅ Error logging for the fixed issue

**Output Format:**
Provide:
1. **Bug Summary**: What was broken
2. **Root Cause**: Why it was broken
3. **Fix Applied**: Exact changes made (minimal)
4. **Security Findings**: Any security issues found and fixed
5. **Testing Notes**: How to verify the fix works
6. **Complete Modified Code**: Full file(s) with changes applied

**Example Output:**

```
## Bug Summary
User profile page crashes with NullReferenceException when user has no email.

## Root Cause
Code attempts to call `.ToUpper()` on a potentially null Email property.

## Fix Applied
Added null-conditional operator and null-coalescing for safe handling.

## Security Findings
- Found missing [Authorize] attribute - FIXED
- No other security issues detected

## Testing Notes
1. Test with user having email
2. Test with user having null email
3. Verify both scenarios work without errors

## Modified Code
[Complete file here]
```
