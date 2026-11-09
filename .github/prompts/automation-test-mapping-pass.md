# AUTOMATION MAPPING PASS — xUnit + Playwright + API + Traceability

You are a senior SDET and test automation architect.

You are given:
1) A System Inventory
2) A full set of Azure DevOps Test Cases (CSV structure)

Your job is to convert those manual test cases into:

- Automation candidates
- xUnit test scaffolding
- Playwright UI test scaffolding
- API integration test scaffolding
- A traceability matrix (Manual → Automation mapping)

Do NOT regenerate manual test cases.
Do NOT invent features not in the inventory.
Base everything strictly on the provided material.

---

# PART 1 — AUTOMATION STRATEGY SUMMARY

Provide a short strategy section that defines:

## 1. Test Pyramid Distribution
Estimate:
- Unit tests %
- Integration tests %
- UI tests %
- Non-automatable %

Explain reasoning based on:
- Risk
- Business logic density
- UI complexity
- API surface

---

## 2. Automation Boundaries

Identify:
- What should be unit tested
- What should be integration tested
- What should be UI tested
- What should remain manual
- What should be contract tested

---

# PART 2 — TRACEABILITY MATRIX

Create a structured mapping table (Markdown format):

| Manual Test Case Title | Automation Type | Test Project | Priority | Notes |
|------------------------|----------------|--------------|----------|-------|

Automation Type values:
- Unit (xUnit)
- Integration (WebApplicationFactory)
- API Integration
- Playwright UI
- Manual Only

Priority:
- P0 (Critical regression)
- P1 (High value)
- P2 (Normal)
- P3 (Low)

Base prioritization on:
- Financial impact
- Permission enforcement
- State transitions
- Destructive operations
- High defect risk areas

---

# PART 3 — PROJECT STRUCTURE RECOMMENDATION

Recommend a .NET test solution structure such as:

/tests
  /ProjectName.UnitTests
  /ProjectName.IntegrationTests
  /ProjectName.ApiTests
  /ProjectName.UITests

Include:
- Suggested NuGet packages
- Base test classes
- Folder structure per feature
- Naming conventions

---

# PART 4 — xUNIT UNIT TEST SCAFFOLDING

For each major service or business logic class identified:

Generate:

- Test class skeleton
- Constructor setup
- Example test methods covering:
  - Happy path
  - Validation
  - Exception case
  - Boundary condition

Example format:

public class ProjectServiceTests
{
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        // Setup mocks
    }

    [Fact]
    public void CloseProject_ValidRequest_ChangesStatusToClosed()
    {
        // Arrange

        // Act

        // Assert
    }
}

Keep scaffolding realistic and aligned with inventory.

---

# PART 5 — INTEGRATION TEST SCAFFOLDING (WebApplicationFactory)

Generate:

- CustomWebApplicationFactory example
- Sample integration test using HttpClient
- Example authenticated request setup
- Example role-based authorization test

Use realistic route patterns from inventory.

---

# PART 6 — PLAYWRIGHT UI TEST SCAFFOLDING

Generate:

- BaseTest class
- Authentication helper
- Example UI test per major feature

Follow pattern:

test('Projects - Close Project - Status updates', async ({ page }) => {
    // Arrange

    // Act

    // Assert
});

Include:
- Locator best practices
- Role-based UI visibility test example
- Validation error test example

---

# PART 7 — CI/CD PIPELINE INTEGRATION

Recommend:

- How to run:
  - dotnet test
  - Playwright tests
- Parallelization strategy
- Test categorization (Trait attributes)
- Azure DevOps YAML snippet example
- How to publish:
  - TRX
  - Playwright reports

---

# PART 8 — REGRESSION SUITE DEFINITION

Define:

## Smoke Suite
Minimal tests that must pass before deployment.

## Full Regression Suite
Complete automated coverage.

Map specific high-priority tests into Smoke.

---

# RULES

- Do not repeat full manual test steps.
- Do not generate thousands of lines of code — generate representative scaffolding.
- Keep examples realistic and aligned with inventory.
- Prefer maintainable structure over clever patterns.
- Avoid hypothetical frameworks not already implied by stack.

---

Begin using the provided System Inventory and Test Case CSV.