---
model: gpt-4o
description: Refactors existing legacy code while preserving implementation style and patterns.
---

@legacy-refactor

I need to refactor existing code: {{input}}

**Critical Rules:**
- **PRESERVE existing style**: If the code uses `<EditForm>`, keep `<EditForm>`
- **PRESERVE component types**: `<InputText>` stays `<InputText>`, raw HTML stays raw HTML
- **PRESERVE CSS approach**: Isolated CSS stays isolated, inline stays inline
- **PRESERVE route paths**: Don't change URLs without explicit permission
- **IMPROVE logic**: Extract business logic, add error handling, fix security issues
- **ENFORCE security**: Apply security protocols regardless of existing patterns

**Your Task:**

1. **Analyze Current Implementation**:
   - Identify component pattern (EditForm, raw HTML, hybrid)
   - Review data flow (services, state management)
   - Note CSS approach (isolated, inline, external)
   - Check error handling patterns
   - Examine validation approach

2. **Plan Improvements** (Logic & Security Only):
   - Extract complex business logic to services
   - Move logic from `@code` blocks to injected services
   - Add null safety and nullable annotations
   - Add/improve try-catch blocks with logging
   - Optimize database queries (`.AsNoTracking()` for reads)
   - Fix security vulnerabilities

3. **Refactor While Preserving Style**:
   - Keep same component libraries (EditForm stays EditForm)
   - Keep same HTML structure
   - Keep same CSS class names
   - Keep same method signatures exposed to other components
   - Keep same route paths

4. **Security & Quality Audit**:
   - Run `blazor-security` skill
   - Run `legacy-aware-audit` skill to verify style preservation
   - Run `security-audit` skill
   - Fix all security issues discovered
   - Ensure `@attribute [Authorize]` is present (unless explicitly public)

5. **Validate**:
   - Ensure no breaking changes to component interface
   - Verify existing callers still work unchanged
   - Confirm identical user-facing behavior

**What NOT to Change:**
- ❌ Component library choices (EditForm → raw HTML)
- ❌ HTML element types or structure
- ❌ CSS approach or class names
- ❌ Route paths or URLs
- ❌ Public method signatures

**What TO Change:**
- ✅ Business logic extraction to services
- ✅ Null safety and error handling
- ✅ Async/await optimization
- ✅ Security attributes and validation
- ✅ Database query optimization
- ✅ Logging addition

**Output:**
Provide:
1. Summary of improvements made
2. Complete refactored code
3. Security audit results
4. Confirmation that style/behavior is preserved
