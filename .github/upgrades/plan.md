# .NET 10.0 Upgrade Plan

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Migration Strategy](#migration-strategy)
3. [Detailed Dependency Analysis](#detailed-dependency-analysis)
4. [Project-by-Project Plans](#project-by-project-plans)
   - [ftodashboard.csproj](#ftodashboardcsproj)
   - [ftodashboard.Tests.csproj](#ftodashboardtestscsproj)
5. [Package Update Reference](#package-update-reference)
6. [Breaking Changes Catalog](#breaking-changes-catalog)
7. [Risk Management](#risk-management)
8. [Complexity & Effort Assessment](#complexity--effort-assessment)
9. [Testing & Validation Strategy](#testing--validation-strategy)
10. [Source Control Strategy](#source-control-strategy)
11. [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description

Upgrade FTO Dashboard solution from **.NET 8.0** to **.NET 10.0 (Long Term Support)** to leverage the latest platform capabilities, performance improvements, and extended support lifecycle.

### Scope

**Projects Affected**: 2 projects
- `ftodashboard\ftodashboard.csproj` - ASP.NET Core Blazor web application
- `ftodashboard.Tests\ftodashboard.Tests.csproj` - xUnit test project

**Current State**: Both projects target `net8.0`

**Target State**: Both projects will target `net10.0`

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in a single atomic operation.

**Rationale**:
- Small solution (2 projects, ~4K LOC)
- Simple dependency structure (single tier: app ? test)
- Both projects currently on modern .NET (net8.0)
- Low complexity ratings for both projects
- All required packages have clear upgrade paths
- No security vulnerabilities present

### Discovered Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Projects | 2 | All require upgrade |
| Dependency Depth | 1 level | Simple hierarchy |
| Total NuGet Packages | 22 | 6 need upgrade, 1 deprecated |
| Total LOC | 3,936 | Small codebase |
| Files with Incidents | 5 | Low impact area |
| Estimated LOC to Modify | 9+ | <0.3% of codebase |
| API Issues | 9 | 2 binary, 3 source, 4 behavioral |
| Security Vulnerabilities | 0 | ? None detected |

### Complexity Classification

**Simple Solution** - Meets all criteria for streamlined upgrade:
- ? ?5 projects (2 projects)
- ? Dependency depth ?2 (1 level)
- ? No high-risk projects
- ? No security vulnerabilities
- ? Modern starting framework (net8.0)

### Critical Issues

**Deprecated Package**:
- `Azure.Identity` 1.14.1 - requires attention (check for replacement guidance)

**API Breaking Changes**:
- 2 Binary Incompatible APIs (require code changes)
- 3 Source Incompatible APIs (compilation fixes needed)
- 4 Behavioral Changes (runtime testing required)

### Iteration Strategy

Using **Fast Batch** approach (2-3 iterations):
1. ? Foundation setup (dependency analysis, strategy, project stubs)
2. ?? Batch all project details together (simple solution)
3. ?? Final polish (success criteria, source control)

### Recommended Approach

**Single Atomic Upgrade**: Update all project files, all packages, restore, build, fix compilation errors, and verify in one coordinated operation. Test validation follows as a separate phase.

---

## Migration Strategy

### Approach Selection

**Selected: All-At-Once Strategy**

This migration will update both projects simultaneously in a single, coordinated operation rather than incrementally migrating one project at a time.

### Justification

**Why All-At-Once is Optimal**:

1. **Small Solution Size**: With only 2 projects (~4K LOC), the scope is manageable as a single unit
2. **Simple Dependencies**: Single-level dependency chain (test ? app) eliminates coordination complexity
3. **Modern Starting Point**: Both projects already on net8.0, making the jump to net10.0 straightforward
4. **Low Risk Profile**: 
   - No security vulnerabilities
   - Both projects rated "Low" complexity
   - Only 9 API issues affecting <0.3% of codebase
   - All packages have clear upgrade paths
5. **Faster Completion**: Eliminates multi-targeting complexity and intermediate testing cycles
6. **Consistent State**: No period where projects are on different framework versions

**Why NOT Incremental**:
- Too few projects to benefit from phasing
- No high-risk components requiring isolation
- Simple dependency structure doesn't justify sequential approach
- Would add unnecessary overhead (multi-targeting, partial testing)

### All-At-Once Strategy Rationale

This strategy treats the upgrade as an **atomic transaction**:
- All project files updated to `net10.0` in one pass
- All package versions updated together
- Single restore ? build ? fix ? verify cycle
- No intermediate states to maintain

**Key Principle**: Since the test project depends on the main app, upgrading them together ensures immediate compatibility and avoids the complexity of maintaining a net8.0 test project testing a net10.0 application.

### Dependency-Based Ordering

While updates happen simultaneously, the **logical validation order** respects dependencies:

1. **Update Phase** (simultaneous):
   - Update `ftodashboard.csproj` TargetFramework to `net10.0`
   - Update `ftodashboard.Tests.csproj` TargetFramework to `net10.0`
   - Update all package references in both projects

2. **Build Phase** (dependency-respecting):
   - Build foundation project first: `ftodashboard.csproj`
   - Fix compilation errors in foundation
   - Build dependent project: `ftodashboard.Tests.csproj`
   - Entire solution builds with 0 errors

3. **Test Phase**:
   - Run tests in `ftodashboard.Tests.csproj`
   - Verify all tests pass

### Execution Approach

**Single Coordinated Operation** with these characteristics:

- **No Multi-Targeting**: Projects go directly from `net8.0` to `net10.0`
- **No Partial Builds**: Solution builds as a complete unit after all updates
- **No Incremental Testing**: Tests run after all projects successfully compile
- **Single Commit**: All changes committed together (atomic source control)

### Risk Mitigation for All-At-Once

**Preparation**:
- ? All work on dedicated branch (`upgrade-to-NET10`)
- ? Comprehensive assessment completed
- ? Breaking changes catalog prepared
- ? Package compatibility verified

**Safety Net**:
- Easy rollback via Git branch (no merged partial states)
- Known set of breaking changes to address
- Clear validation criteria (build + tests pass)

**Validation Strategy**:
- Build entire solution after updates
- Address all compilation errors in one session
- Run full test suite as final verification
- No deployment until all validation passes

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution has a clean, two-tier dependency structure:

```
Level 0 (Foundation):
?? ftodashboard.csproj (ASP.NET Core web app)
   ?? Used by: ftodashboard.Tests

Level 1 (Test Layer):
?? ftodashboard.Tests.csproj (Test project)
   ?? Depends on: ftodashboard
```

**Visual Representation**:
```
ftodashboard.Tests.csproj (net8.0)
         ?
ftodashboard.csproj (net8.0)
```

### Project Groupings by Migration Phase

Since this is an **All-At-Once Strategy**, all projects upgrade simultaneously in a single phase:

**Phase 1: Atomic Upgrade** (All Projects)
- `ftodashboard\ftodashboard.csproj` - Main application (Level 0)
- `ftodashboard.Tests\ftodashboard.Tests.csproj` - Test project (Level 1)

**Rationale**: With only 2 projects and a simple dependency chain, there's no benefit to sequential migration. Both projects can be updated atomically, ensuring consistency and minimizing the time window where projects are on different framework versions.

### Critical Path Identification

**Critical Path**: `ftodashboard.csproj` ? `ftodashboard.Tests.csproj`

While both projects upgrade simultaneously, the main application is the foundation:
- **ftodashboard.csproj** contains all API breaking changes and package updates
- **ftodashboard.Tests.csproj** has no API issues, only framework and package updates

**Key Insight**: The main application drives the complexity; the test project follows naturally once the application compiles successfully.

### Circular Dependencies

**None Detected** ? - Clean dependency hierarchy with no cycles.

### Dependency Constraints

For All-At-Once strategy:
- Both project files update to `net10.0` simultaneously
- All package updates apply in the same operation
- Solution-wide restore and build after all changes applied
- No intermediate multi-targeting needed

---

## Project-by-Project Plans

### ftodashboard.csproj

**Current State**: 
- Target Framework: `net8.0`
- Project Type: ASP.NET Core Blazor Server
- Dependencies: 0 project dependencies
- Dependants: 1 (ftodashboard.Tests)
- LOC: 3,271
- Packages: 17 packages
- Risk Level: ?? Medium (contains all API breaking changes)

**Target State**: 
- Target Framework: `net10.0`
- Updated Packages: 6 packages (5 required + 1 deprecated)

---

#### Migration Steps

##### 1. Prerequisites

**Verify Dependencies**: None - this is the foundation project with no project dependencies.

**Tooling Check**: Ensure .NET 10.0 SDK installed.

##### 2. Technology/Framework Update

**Project File Change** (`ftodashboard\ftodashboard.csproj`):

Update the `TargetFramework` property:

```xml
<TargetFramework>net10.0</TargetFramework>
```

**No other project file changes required** - all other properties compatible with net10.0.

##### 3. Package/Dependency Updates

Update the following PackageReference elements in `ftodashboard\ftodashboard.csproj`:

| Package | Current Version | Target Version | Priority |
|---------|----------------|----------------|----------|
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 8.0.18 | 10.0.1 | High (framework alignment) |
| Microsoft.EntityFrameworkCore | 9.0.7 | 10.0.1 | High (framework alignment) |
| Microsoft.EntityFrameworkCore.Relational | 9.0.7 | 10.0.1 | High (framework alignment) |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.7 | 10.0.1 | High (framework alignment) |
| Azure.Identity | 1.14.1 | 1.17.1 | High (deprecated MSAL fix) |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Medium (bug fixes) |

**Update All 6 Packages** as part of atomic upgrade operation.

##### 4. Expected Breaking Changes

**File**: `ftodashboard\Program.cs` (all 9 API issues are in this file)

**Binary Incompatible** (require code changes):
1. **Line 11** - `ConfigurationBinder.GetValue<T>()` - Update to new API pattern or use GetSection().Get<T>()
2. **Line 70** - `AddTelerikBlazor()` - Verify Telerik compatibility; may need API adjustment

**Source Incompatible** (require compilation fixes):
3. **Line 33** - `AddAzureKeyVault()` - Verify extension method availability after Azure.Identity update
4. **Line 14** - `OpenIdConnectDefaults` type - Verify namespace/type after package update
5. **Line 14** - `OpenIdConnectDefaults.AuthenticationScheme` - Verify field after package update

**Behavioral Changes** (require runtime testing):
6. **Line 82** - `UseExceptionHandler("/Error")` - Test exception handling
7. **Line 73** - `AddHttpClient()` - Test HTTP client behavior
8. **Line 33** - `new Uri(...)` - Test URI construction
9. **Line 33** - `System.Uri` type - Test Azure Key Vault connectivity

**See Breaking Changes Catalog** for detailed fix guidance for each issue.

##### 5. Code Modifications

**Primary Focus**: `ftodashboard\Program.cs`

**Expected Changes**:
- Update `ConfigurationBinder.GetValue<T>()` usage (Line 11)
- Verify/fix `AddTelerikBlazor()` call (Line 70)
- Verify `AddAzureKeyVault()` extension method (Line 33)
- Verify `OpenIdConnectDefaults` type and field (Line 14)
- Add any missing namespace imports if APIs moved

**Configuration Files**: No changes expected to `appsettings.json` or other config files.

**Other Code Areas**: Review compilation errors; breaking changes concentrated in `Program.cs`.

##### 6. Testing Strategy

**Build Validation**:
- [ ] Project compiles without errors
- [ ] Project compiles without warnings
- [ ] No package dependency conflicts

**Unit Testing**:
- Test project (`ftodashboard.Tests`) will run tests after this project builds

**Integration Testing**:
- [ ] Application starts successfully
- [ ] Azure Key Vault configuration loads (Line 33 URI changes)
- [ ] Authentication works (OpenIdConnect changes, Line 14)
- [ ] Exception handling behaves correctly (Line 82)
- [ ] HTTP client requests work (Line 73)

**Manual Testing Areas**:
- Azure AD authentication flow
- Exception handling (navigate to error scenarios)
- HTTP client functionality (if app makes external calls)
- Telerik Blazor components render correctly

##### 7. Validation Checklist

- [ ] `ftodashboard.csproj` TargetFramework is `net10.0`
- [ ] All 6 package updates applied with correct versions
- [ ] `dotnet restore` completes successfully
- [ ] Project builds without errors (`dotnet build`)
- [ ] Project builds without warnings
- [ ] No package version conflicts
- [ ] No deprecated package warnings (Azure.Identity updated to 1.17.1)
- [ ] All API breaking changes in `Program.cs` addressed
- [ ] Application starts without runtime errors
- [ ] Azure Key Vault connectivity verified
- [ ] Authentication flow works
- [ ] Exception handling tested

---

### ftodashboard.Tests.csproj

**Current State**: 
- Target Framework: `net8.0`
- Project Type: xUnit Test Project
- Dependencies: 1 (ftodashboard)
- Dependants: 0
- LOC: 665
- Packages: 6 packages
- Risk Level: ?? Low (no API issues)

**Target State**: 
- Target Framework: `net10.0`
- Updated Packages: 1 package

---

#### Migration Steps

##### 1. Prerequisites

**Verify Dependencies**: `ftodashboard.csproj` must be upgraded and building successfully before this project can upgrade.

**Tooling Check**: Same .NET 10.0 SDK as main project.

##### 2. Technology/Framework Update

**Project File Change** (`ftodashboard.Tests\ftodashboard.Tests.csproj`):

Update the `TargetFramework` property:

```xml
<TargetFramework>net10.0</TargetFramework>
```

**No other project file changes required**.

##### 3. Package/Dependency Updates

Update the following PackageReference in `ftodashboard.Tests\ftodashboard.Tests.csproj`:

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.EntityFrameworkCore.InMemory | 9.0.7 | 10.0.1 | Framework alignment with net10.0 |

**All Other Packages Remain Compatible**:
- Microsoft.EntityFrameworkCore 9.0.7 (inherited from main project - already updated there)
- Microsoft.NET.Test.Sdk 17.14.1 ?
- Moq 4.20.72 ?
- xunit 2.9.3 ?
- xunit.runner.visualstudio 3.1.3 ?

##### 4. Expected Breaking Changes

**None** ? - No API breaking changes detected in this project.

Test code may require updates if:
- Main application API changes affect test assertions
- Entity Framework InMemory provider behavior changes
- Test framework behavior changes (unlikely)

##### 5. Code Modifications

**Expected**: Minimal to none.

**Potential Areas** (check after main project upgrade):
- Update test assertions if `ftodashboard` public APIs changed
- Verify DbContext setup with EF Core 10.0.1 InMemory provider
- Check for any test failures due to behavioral changes in dependencies

##### 6. Testing Strategy

**Build Validation**:
- [ ] Project compiles without errors
- [ ] Project compiles without warnings
- [ ] No package dependency conflicts with upgraded main project

**Test Execution**:
- [ ] All tests run successfully
- [ ] All tests pass
- [ ] No test failures due to framework upgrade
- [ ] No test failures due to EF InMemory provider changes

**Test Framework Compatibility**:
- Verify xUnit runner works with .NET 10.0
- Verify test discovery works in IDE and CLI
- Verify Moq mocking library compatible with .NET 10.0

##### 7. Validation Checklist

- [ ] `ftodashboard.Tests.csproj` TargetFramework is `net10.0`
- [ ] `Microsoft.EntityFrameworkCore.InMemory` updated to 10.0.1
- [ ] `dotnet restore` completes successfully
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] No package version conflicts
- [ ] Tests discovered successfully (`dotnet test --list-tests`)
- [ ] All tests execute (`dotnet test`)
- [ ] All tests pass (0 failures)
- [ ] Test coverage maintained or improved

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

This upgrade follows a **three-tier validation strategy**: per-project validation ? phase testing ? full solution verification.

---

### Level 1: Per-Project Testing

After upgrading each project, validate in isolation before proceeding.

#### ftodashboard.csproj Validation

**Build Validation**:
- [ ] No package dependency conflicts
- [ ] `dotnet restore` completes successfully
- [ ] `dotnet build ftodashboard\ftodashboard.csproj` succeeds (0 errors)
- [ ] No compiler warnings related to upgrade
- [ ] No deprecated package warnings (Azure.Identity 1.17.1 resolves deprecation)

**Code Quality**:
- [ ] All API breaking changes in `Program.cs` addressed
- [ ] No new warnings introduced
- [ ] Code analysis rules still satisfied

**Runtime Validation**:
- [ ] Application starts (`dotnet run`)
- [ ] No startup errors in console output
- [ ] Azure Key Vault configuration loads successfully
- [ ] Azure AD authentication initialization succeeds

#### ftodashboard.Tests.csproj Validation

**Build Validation**:
- [ ] Project references upgraded `ftodashboard.csproj` successfully
- [ ] `dotnet restore` completes successfully
- [ ] `dotnet build ftodashboard.Tests\ftodashboard.Tests.csproj` succeeds (0 errors)
- [ ] No warnings related to test framework compatibility

**Test Discovery**:
- [ ] Tests discovered: `dotnet test ftodashboard.Tests\ftodashboard.Tests.csproj --list-tests`
- [ ] All expected tests found (no missing tests)

**Test Execution**:
- [ ] All tests execute: `dotten
