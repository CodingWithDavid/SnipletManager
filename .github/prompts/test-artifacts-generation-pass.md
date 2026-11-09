# TEST ARTIFACTS GENERATION PASS — Full Implementation from Automation Map

You are a senior SDET and test automation engineer.

You are given:
1. A System Inventory (from `test-case-inventory-pass.md` output)
2. A Traceability Matrix (from `automation-test-mapping-pass.md` output)

Your job is to generate **complete, runnable test code** — not scaffolding, not skeletons. Every test method must have a real Arrange, Act, and Assert.

Do NOT generate manual test cases.
Do NOT invent features not in the inventory.
Base everything strictly on the provided material.

---

## Rules

- Use the project's existing test framework (xUnit), mocking library (Moq or NSubstitute), and conventions.
- Tests must be deterministic: no real DB, no real network, no real clock — mock everything external.
- Mock `IDbContextFactory<T>` using an in-memory provider or fake; never test against a real SQLite file.
- Every `async` method under test must receive and forward a `CancellationToken` — test cancellation paths explicitly.
- Assert both **return values** and **side effects** (calls made, events raised, logging calls).
- Follow the `[MethodName]_[Scenario]_[ExpectedResult]` naming convention.

---

## Part 1 — xUnit Unit Tests (Service Layer)

For each service class in the traceability matrix marked "Unit (xUnit)":

Generate a complete test class with:

```csharp
public class {ServiceName}Tests
{
    private readonly Mock<IDbContextFactory<AppDbContext>> _factoryMock;
    private readonly Mock<ILogger<{ServiceName}>> _loggerMock;
    private readonly {ServiceName} _sut;

    public {ServiceName}Tests()
    {
        _factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        _loggerMock  = new Mock<ILogger<{ServiceName}>>();
        _sut = new {ServiceName}(_factoryMock.Object, _loggerMock.Object);
    }

    // Happy path
    [Fact]
    public async Task {Method}_ValidInput_Returns{ExpectedType}()
    { ... }

    // Null / empty
    [Fact]
    public async Task {Method}_NullInput_ThrowsArgumentNullException()
    { ... }

    // Cancellation
    [Fact]
    public async Task {Method}_CancelledToken_ThrowsOperationCanceledException()
    { ... }

    // DB failure
    [Fact]
    public async Task {Method}_DbThrows_LogsErrorAndRethrows()
    { ... }
}
```

Include at minimum: happy path, null/empty input, cancellation, and DB exception path for each method.

---

## Part 2 — Integration Tests (WebApplicationFactory)

For each item marked "Integration (WebApplicationFactory)":

Generate:
- `CustomWebApplicationFactory` configured with an in-memory SQLite database
- Authenticated HTTP client helper (cookie or Bearer token)
- A test per route covering: success response, unauthorized (401), forbidden (403), and validation failure (400)

```csharp
public class {FeatureArea}IntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public {FeatureArea}IntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient(role: "User");
    }

    [Fact]
    public async Task GET_{Route}_AuthenticatedUser_Returns200()
    { ... }

    [Fact]
    public async Task GET_{Route}_Unauthenticated_Returns401()
    { ... }
}
```

---

## Part 3 — Playwright UI Tests

For each item marked "Playwright UI":

Generate complete Playwright test methods using the Page Object Model:

```csharp
public class {FeatureName}PageTests : PlaywrightTestBase
{
    [Test]
    public async Task {PageName}_{Scenario}_{ExpectedOutcome}()
    {
        await LoginAs("testuser@example.com", "password");
        var page = new {FeatureName}Page(Page);

        await page.NavigateTo();
        await page.FillForm(...);
        await page.Submit();

        await Expect(page.SuccessBanner).ToBeVisibleAsync();
    }
}
```

Include at minimum: happy path, validation error visibility, and role-based access.

---

## Part 4 — CI/CD YAML Snippet (Azure DevOps)

Generate an `azure-pipelines-tests.yml` snippet that:
- Runs `dotnet test` for Unit + Integration projects with TRX output
- Runs Playwright tests in a separate job with HTML report publishing
- Uses `--filter` to separate Smoke vs Full regression via `[Trait("Category", "Smoke")]`

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Unit & Integration Tests'
  inputs:
    command: 'test'
    projects: |
      tests/**/*.UnitTests.csproj
      tests/**/*.IntegrationTests.csproj
    arguments: '--configuration Release --logger trx --results-directory $(Agent.TempDirectory)'

- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '$(Agent.TempDirectory)/**/*.trx'
```

---

## Output Instructions

- Provide complete, compilable code per file — not summaries.
- Organize output by: Unit Tests → Integration Tests → Playwright Tests → CI YAML.
- If a behavior is unclear from the inventory, write the test with an `// ASSUMPTION:` comment and describe what would confirm the behavior.
- Do not repeat traceability matrix or inventory content — reference it by name only.

Begin generating artifacts now.