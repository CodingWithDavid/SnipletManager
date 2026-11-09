---
model: gpt-4o
description: Analyzes a legacy Blazor codebase to discover and document existing patterns and conventions.
---

@legacy-analyzer

Analyze this legacy Blazor codebase and document its patterns and conventions.

**Your Task:**
Perform a comprehensive scan of the codebase to understand existing patterns so new features can integrate properly.

**Analysis Areas:**

1. **Project Structure**
   - Locate where Pages, Components, Services, Data/Models are organized
   - Identify if a layered architecture is used
   - Note any custom folder conventions

2. **Component Patterns**
   - Identify form approach: `<EditForm>` with Blazor inputs OR raw HTML with `@bind`
   - Identify data display: `<QuickGrid>`, HTML tables, or custom components
   - Check validation approach: `<DataAnnotationsValidator>` or manual
   - Determine render mode usage: `@rendermode InteractiveServer`, SSR, or mixed
   - Identify CSS approach: Isolated (`.razor.css`), inline styles, or external CSS

3. **Service & Dependency Injection Patterns**
   - Examine `Program.cs` for service registration patterns
   - Identify naming conventions (Service, Repository, Manager suffixes)
   - Determine if Repository pattern is used
   - Check if interfaces are consistently used

4. **Data Access Patterns**
   - Check if `DbContext` is injected directly OR `IDbContextFactory<T>` is used
   - Identify if Repository pattern or Unit of Work is used
   - Locate EF migrations folder
   - Find connection string name

5. **Routing & Navigation**
   - Analyze route path casing patterns (`/admin/users` vs `/Admin/Users`)
   - Check navigation approach (`NavigationManager` vs anchor tags)
   - Identify layout usage patterns

6. **Security Patterns**
   - Review `@attribute [Authorize]` usage patterns
   - Check if role-based or policy-based authorization is used
   - Identify any custom authentication handlers

7. **Naming Conventions**
   - Document casing standards (PascalCase, camelCase)
   - List common suffixes used (Service, Repository, Manager, Handler)
   - Note file naming patterns

**Output:**
Create a structured analysis document at `.github/analysis/codebase-analysis.md` with:
- All findings organized by category
- Specific examples from actual code
- Recommendations for integrating new features
- List of key files examined as evidence

**Analysis Depth:**
Perform a **Standard Scan** (20-30 representative files) unless specified otherwise.

**Template Structure:**

```markdown
# Legacy Codebase Analysis

**Analysis Date**: [Date]
**Project**: [Project Name]
**Files Examined**: [Count]

---

## 1. Project Structure
[Folder organization with paths]

## 2. Component Patterns
[Forms, data display, validation, render modes, CSS]

## 3. Service Patterns
[Registration style, naming, interfaces, patterns]

## 4. Data Access
[DbContext usage, patterns, migrations, connection strings]

## 5. Routing Conventions
[Path casing, navigation style]

## 6. Security Patterns
[Authorization approach]

## 7. Naming Conventions
[Casing, suffixes, file naming]

## 8. Integration Recommendations
[Key guidance for other agents]

---

**Evidence Files**:
- [List of examined files]
```

Begin analysis now.
