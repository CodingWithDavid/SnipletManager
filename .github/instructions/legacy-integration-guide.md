# Legacy Integration Guide

## Purpose
This guide explains how to work with existing legacy Blazor applications while maintaining consistency with greenfield development standards.

---

## Project Type Detection

**When starting any work, determine the project type:**

### Greenfield Project Indicators:
- New project scaffolding
- Empty or minimal codebase
- User explicitly requests "new application"

### Legacy Project Indicators:
- Existing Pages, Components, Services folders with content
- Established patterns and conventions
- User mentions "existing app", "legacy", "current project"

**If unclear, ask the user**: *"Is this a new greenfield project or an existing legacy application?"*

---

## Working with Legacy Projects

### Step 1: Always Analyze First
Before making any changes to a legacy project:
1. Use the `@legacy-analyzer` agent to scan the codebase
2. Review the generated `.github/analysis/codebase-analysis.md`
3. Understand existing patterns: component style, service patterns, folder structure, naming conventions

**Quick Manual Analysis (if not using analyzer):**
- Check 3-5 existing Pages to identify component patterns
- Review `Program.cs` to understand service registration
- Examine folder structure for organizational conventions

---

## Three Legacy Modes

### Mode 1: Adding New Features (Use `legacy-add-feature.prompt.md`)

**When to Use:**
- Adding entirely new functionality to existing app
- Creating new pages or components
- Building new features from scratch

**Rules:**
- ✅ **New code follows greenfield standards**: Raw HTML, QuickGrid, isolated CSS
- ✅ **Integration respects existing patterns**: Folder structure, routing, naming
- ✅ **Coexistence is allowed**: New raw HTML components can coexist with existing EditForm pages
- ✅ **Security is mandatory**: Run security audits on all new code

**Example:**
Adding a new "Product Catalog" feature to an existing ERP system that uses EditForm:
- New Product pages use raw HTML forms (per greenfield standards)
- Place in existing `/Pages/Products/` folder (per legacy convention)
- Register services following existing DI pattern in `Program.cs`
- Both old EditForm pages and new raw HTML pages work together

---

### Mode 2: Refactoring Existing Code (Use `legacy-refactor.prompt.md`)

**When to Use:**
- Improving existing code structure
- Extracting business logic to services
- Optimizing performance
- Cleaning up technical debt

**Rules:**
- ❌ **NEVER change component patterns**: EditForm stays EditForm, raw HTML stays raw HTML
- ❌ **NEVER change CSS approach**: Keep isolated, inline, or external as-is
- ❌ **NEVER change route paths**: URLs must remain the same
- ✅ **DO improve logic**: Extract to services, add error handling, optimize queries
- ✅ **DO enforce security**: Fix security issues regardless of existing patterns

**Example:**
Refactoring an existing User Management page that uses EditForm:
- Keep `<EditForm>` and `<InputText>` components
- Extract user validation logic to `UserService`
- Add proper null checks and error handling
- Fix missing `[Authorize]` attribute (security issue)
- Do NOT convert to raw HTML

---

### Mode 3: Fixing Bugs (Use `legacy-bug-fix.prompt.md`)

**When to Use:**
- Fixing specific bugs or errors
- Resolving crashes or incorrect behavior
- Addressing reported issues

**Rules:**
- ✅ **Surgical precision**: Change ONLY what's needed to fix the bug
- ✅ **Match existing style**: Use exact same patterns and conventions
- ✅ **Security is mandatory**: Run audits and fix ANY security issues found
- ❌ **NO refactoring**: Don't improve unrelated code
- ❌ **NO style changes**: Don't modernize to greenfield standards

**Example:**
Fixing NullReferenceException in existing Login page:
- Add null check for the specific property
- Match surrounding code style exactly
- Run security audit (discovers missing rate limiting)
- Fix both the bug AND the security issue
- Do NOT refactor the entire login flow

---

## Security Enforcement (All Modes)

**Non-Negotiable Security Rules** (apply to ALL legacy work):
1. Always run `blazor-security` skill on modified code
2. Always run `security-audit` skill
3. Fix ANY security issues discovered, even if unrelated to your task
4. Add `@attribute [Authorize]` if missing (unless explicitly public)
5. Fix XSS vulnerabilities immediately
6. Fix SQL injection risks immediately
7. Ensure proper input validation

---

## Integration Best Practices

### Folder Structure
- **Respect existing conventions**: If Pages are in `/Pages/Admin/`, put new admin pages there
- **Don't reorganize**: Avoid moving existing files during feature addition

### Service Registration
- **Match existing patterns**: If they use `AddScoped<IUserService, UserService>()`, follow that style
- **Place in Program.cs**: Add registrations where existing services are registered

### Routing
- **Follow casing patterns**: If existing routes use `/admin/users`, new routes use same casing
- **Maintain consistency**: Don't mix `/Admin/Users` and `/admin/users` in same app

### Naming Conventions
- **Match suffixes**: If services end in `Service`, name yours `ProductService` not `ProductManager`
- **Match casing**: PascalCase for public, camelCase for private if that's the pattern

### Database Access
- **Use existing pattern**: If they inject `IDbContextFactory<T>`, you do too
- **Follow repository pattern**: If repositories exist, create new ones for new entities
- **Don't mix patterns**: Don't introduce direct EF Core if repositories are the standard

---

## Decision Matrix

| Scenario | Use Mode | Apply Greenfield Standards? | Preserve Existing Style? |
|----------|----------|----------------------------|-------------------------|
| Add new feature | Feature Builder | ✅ Yes (for new code) | ✅ Yes (for integration) |
| Add new page | Feature Builder | ✅ Yes | ✅ Yes (folder/routing) |
| Refactor logic | Refactor | ❌ No | ✅ Yes (all UI/patterns) |
| Extract to service | Refactor | ❌ No | ✅ Yes (naming/registration) |
| Fix crash/bug | Bug Fix | ❌ No | ✅ Yes (everything) |
| Fix security issue | Bug Fix | ❌ No | ✅ Yes (style matching) |
| Optimize query | Refactor | ❌ No | ✅ Yes (all patterns) |

---

## Common Pitfalls to Avoid

### ❌ Don't Do This:
- "While fixing this bug, let me also modernize the form to use raw HTML" → NO
- "This refactor is a good time to switch from EditForm to raw HTML" → NO
- "Let me reorganize the folder structure while adding this feature" → NO
- "I'll skip the security audit since it's just a small change" → NO

### ✅ Do This Instead:
- Fix bugs surgically without style changes
- Refactor logic while preserving UI patterns
- Add features using greenfield standards in new files
- Always run security audits regardless of change size

---

## Quick Reference Commands

```bash
# Analyze codebase before starting
Use prompt: analyze-legacy-codebase.prompt.md

# Add new feature to legacy app
Use prompt: legacy-add-feature.prompt.md

# Refactor existing code
Use prompt: legacy-refactor.prompt.md

# Fix a bug
Use prompt: legacy-bug-fix.prompt.md
```

---

## Summary

- **Analyze first**: Always understand existing patterns before changing code
- **New features = Greenfield standards**: Raw HTML, QuickGrid, isolated CSS
- **Refactoring = Preserve style**: Keep EditForm, keep patterns, improve logic only
- **Bug fixes = Minimal changes**: Surgical fixes with mandatory security audits
- **Security = Always enforced**: No exceptions, fix all issues found
- **Integration = Match conventions**: Folder structure, naming, DI patterns
