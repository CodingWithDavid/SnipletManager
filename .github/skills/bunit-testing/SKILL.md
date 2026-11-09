---
name: bunit-testing
description: |
  Generates bUnit tests for Blazor components with xUnit, NSubstitute, and AwesomeAssertions.
  Covers rendering, parameters, events, DI, JSInterop mocking, NavigationManager, forms, and markup verification.
  WHEN: "bUnit test", "Blazor component test", "test Razor component", "test .razor file",
  "mock IJSRuntime", "mock NavigationManager", "test EditForm", "test cascading parameter",
  "MarkupMatches", "WaitForAssertion".
  DO NOT USE FOR: handler/domain tests (use unit-testing), validator tests (use fluent-validation),
  E2E/browser tests, or non-Blazor UI.
---

# bUnit Test Generator for Blazor Components

## Overview

Generate unit tests for Blazor components using bUnit. bUnit renders components in memory without a browser, making tests fast and reliable. It builds on top of xUnit and integrates with NSubstitute and AwesomeAssertions.

Key capabilities: render components with semantic HTML comparison, pass parameters (simple values, callbacks, cascading values, render fragments), trigger events (click, input, change, submit, custom), inject services via DI, mock JSInterop (strict or loose mode), fake NavigationManager (built-in), and test async behavior (loading states, re-renders).

---

## Test Project Structure

```
tests/
└── {name}.UI.UnitTests/
    ├── Components/
    │   ├── {Feature}/
    │   │   ├── {Component}Tests.cs
    │   │   └── {OtherComponent}Tests.cs
    │   └── Shared/
    │       └── {SharedComponent}Tests.cs
    ├── Abstractions/
    │   └── BlazorTestBase.cs
    ├── _Imports.razor
    └── {name}.UI.UnitTests.csproj
```

---

## Template: Test Project File

```xml
<!-- tests/{name}.UI.UnitTests/{name}.UI.UnitTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="bunit" Version="2.*" />
    <PackageReference Include="AwesomeAssertions" Version="9.4.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\{name}.Web\{name}.Web.csproj" />
  </ItemGroup>

</Project>
```

**Important**: The SDK must be `Microsoft.NET.Sdk.Razor` (not `Microsoft.NET.Sdk`) so that `.razor` test files compile correctly and bUnit can resolve Blazor component types.

---

## Template: _Imports.razor

```razor
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using Microsoft.Extensions.DependencyInjection
@using AngleSharp.Dom
@using Bunit
@using Bunit.TestDoubles
@using Xunit
```

Add `@using` statements for your component namespaces as needed.

---

## Template: Base Test Class

```csharp
// tests/{name}.UI.UnitTests/Abstractions/BlazorTestBase.cs
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace {name}.UI.UnitTests.Abstractions;

/// <summary>
/// Base class for all Blazor component tests.
/// Inherits from BunitContext which provides Render, Services, JSInterop, etc.
/// Each test gets a fresh context because xUnit creates a new instance per test.
/// </summary>
public abstract class BlazorTestBase : BunitContext
{
    /// <summary>
    /// Register a NSubstitute mock in the DI container.
    /// Call this in your test constructor or Arrange phase before rendering.
    /// </summary>
    protected T AddMock<T>() where T : class
    {
        var mock = Substitute.For<T>();
        Services.AddSingleton(mock);
        return mock;
    }

    /// <summary>
    /// Register a concrete service in the DI container.
    /// </summary>
    protected void AddService<TService>(TService instance) where TService : class
    {
        Services.AddSingleton(instance);
    }

    /// <summary>
    /// Configure JSInterop in loose mode so unhandled JS calls
    /// return defaults instead of throwing.
    /// Call in your constructor if the component uses JS heavily.
    /// </summary>
    protected void UseLooseJSInterop()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
```

---

## Template: Basic Component Rendering Test

This pattern covers the most common scenario — rendering a component and checking its markup.

```csharp
using Bunit;
using AwesomeAssertions;
using {name}.Web.Components;
using {name}.UI.UnitTests.Abstractions;

namespace {name}.UI.UnitTests.Components.{Feature};

public sealed class {Component}Tests : BlazorTestBase
{
    [Fact]
    public void Should_RenderCorrectMarkup_When_Rendered()
    {
        // Act
        var cut = Render<{Component}>();

        // Assert - semantic comparison ignores insignificant whitespace,
        // attribute order, and comment differences
        cut.MarkupMatches(@"<h1>Expected Heading</h1>");
    }

    [Fact]
    public void Should_ContainElement_When_Rendered()
    {
        // Act
        var cut = Render<{Component}>();

        // Assert - use Find/FindAll with CSS selectors
        var heading = cut.Find("h1");
        heading.TextContent.Should().Be("Expected Heading");
    }
}
```

---

## Template: Passing Parameters

```csharp
public sealed class GreetingTests : BlazorTestBase
{
    // Simple parameter
    [Fact]
    public void Should_DisplayName_When_NameProvided()
    {
        // Act
        var cut = Render<Greeting>(parameters => parameters
            .Add(p => p.Name, "Alice"));

        // Assert
        cut.Find("p").TextContent.Should().Contain("Alice");
    }

    // Multiple parameters
    [Fact]
    public void Should_ApplyStyles_When_ParametersSet()
    {
        // Act
        var cut = Render<Alert>(parameters => parameters
            .Add(p => p.Message, "Warning!")
            .Add(p => p.Type, AlertType.Warning)
            .Add(p => p.Dismissible, true));

        // Assert
        cut.Find(".alert").ClassList.Should().Contain("alert-warning");
    }

    // EventCallback parameter
    [Fact]
    public void Should_InvokeCallback_When_ButtonClicked()
    {
        // Arrange
        var wasClicked = false;

        var cut = Render<ConfirmButton>(parameters => parameters
            .Add(p => p.OnConfirm, () => { wasClicked = true; }));

        // Act
        cut.Find("button").Click();

        // Assert
        wasClicked.Should().BeTrue();
    }

    // RenderFragment (child content)
    [Fact]
    public void Should_RenderChildContent_When_Provided()
    {
        // Act
        var cut = Render<Card>(parameters => parameters
            .AddChildContent("<p>Card body text</p>"));

        // Assert
        cut.Find(".card-body p").TextContent.Should().Be("Card body text");
    }

    // Cascading parameter
    [Fact]
    public void Should_ReceiveCascadingValue_When_Provided()
    {
        // Act
        var cut = Render<ThemeConsumer>(parameters => parameters
            .Add(p => p.Theme, new AppTheme { IsDark = true }));

        // Assert
        cut.Find(".container").ClassList.Should().Contain("dark-mode");
    }
}
```

---

## Template: Event Handling

```csharp
public sealed class CounterTests : BlazorTestBase
{
    [Fact]
    public void Should_IncrementCount_When_ButtonClicked()
    {
        // Arrange
        var cut = Render<Counter>();

        // Act
        cut.Find("button").Click();

        // Assert
        cut.Find("p").TextContent.Should().Contain("1");
    }

    [Fact]
    public void Should_UpdateText_When_InputChanged()
    {
        // Arrange
        var cut = Render<SearchBox>();

        // Act
        cut.Find("input").Change("search term");

        // Assert
        cut.Find(".search-preview").TextContent.Should().Contain("search term");
    }

    [Fact]
    public void Should_SubmitForm_When_FormSubmitted()
    {
        // Arrange
        var cut = Render<ContactForm>();

        cut.Find("#name-input").Change("John");
        cut.Find("#email-input").Change("john@example.com");

        // Act
        cut.Find("form").Submit();

        // Assert
        cut.Find(".success-message").Should().NotBeNull();
    }

    [Fact]
    public void Should_HandleKeyPress_When_EnterPressed()
    {
        // Arrange
        var cut = Render<SearchBox>();

        // Act
        cut.Find("input").KeyDown(Key.Enter);

        // Assert
        cut.Find(".results").Should().NotBeNull();
    }
}
```

---

## Template: Service Injection

When a component injects a service via `@inject`, register it in the test's DI container before rendering.

```csharp
public sealed class WeatherPageTests : BlazorTestBase
{
    private readonly IWeatherService _weatherService;

    public WeatherPageTests()
    {
        _weatherService = AddMock<IWeatherService>();
    }

    [Fact]
    public void Should_DisplayForecasts_When_DataLoaded()
    {
        // Arrange
        _weatherService.GetForecastsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<WeatherForecast>
            {
                new("2024-01-01", 25, "Sunny"),
                new("2024-01-02", 18, "Cloudy")
            });

        // Act
        var cut = Render<WeatherPage>();

        // Assert - wait for async OnInitializedAsync to complete
        cut.WaitForState(() => cut.FindAll("tr").Count > 1);
        cut.FindAll("tr").Count.Should().Be(3); // header + 2 data rows
    }

    [Fact]
    public void Should_ShowLoading_When_DataNotReady()
    {
        // Arrange - task that never completes
        _weatherService.GetForecastsAsync(Arg.Any<CancellationToken>())
            .Returns(new TaskCompletionSource<List<WeatherForecast>>().Task);

        // Act
        var cut = Render<WeatherPage>();

        // Assert
        cut.Find(".loading-spinner").Should().NotBeNull();
    }
}
```

---

## Template: JSInterop Mocking

bUnit has a built-in JSInterop that intercepts all `IJSRuntime` calls. By default it runs in **strict mode** — any unregistered call throws an exception. Switch to **loose mode** if you want unhandled calls to silently return defaults.

```csharp
public sealed class ClipboardButtonTests : BlazorTestBase
{
    [Fact]
    public void Should_CopyToClipboard_When_Clicked()
    {
        // Arrange - register the expected JS call
        JSInterop.SetupVoid("navigator.clipboard.writeText", "Hello World");

        var cut = Render<ClipboardButton>(p => p
            .Add(c => c.Text, "Hello World"));

        // Act
        cut.Find("button").Click();

        // Assert - verify the JS call happened
        JSInterop.VerifyInvoke("navigator.clipboard.writeText")
            .Arguments[0].Should().Be("Hello World");
    }

    [Fact]
    public void Should_ReturnValue_When_JSReturnsResult()
    {
        // Arrange - setup JS call that returns a value
        JSInterop.Setup<int>("getScrollPosition")
            .SetResult(250);

        // Act
        var cut = Render<ScrollTracker>();

        // Assert
        cut.Find(".scroll-position").TextContent.Should().Contain("250");
    }

    [Fact]
    public void Should_RenderWithoutError_When_LooseModeEnabled()
    {
        // Arrange - loose mode: any unregistered call returns default
        UseLooseJSInterop();

        // Act - component may call any JS, none will throw
        var cut = Render<RichEditor>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }
}
```

---

## Template: NavigationManager

bUnit provides a fake `NavigationManager` automatically — no extra setup needed.

```csharp
public sealed class LoginRedirectTests : BlazorTestBase
{
    [Fact]
    public void Should_NavigateToHome_When_LoginSucceeds()
    {
        // Arrange
        var authService = AddMock<IAuthService>();
        authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        var cut = Render<LoginPage>();

        cut.Find("#username").Change("admin");
        cut.Find("#password").Change("secret");

        // Act
        cut.Find("form").Submit();

        // Assert
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        navManager.Uri.Should().EndWith("/home");
    }
}
```

---

## Template: Testing Async Re-renders

Components often re-render after async operations. Use `WaitForState` or `WaitForAssertion` to wait for the component to reach the expected state.

```csharp
public sealed class AsyncDataGridTests : BlazorTestBase
{
    [Fact]
    public void Should_ShowData_When_AsyncLoadCompletes()
    {
        // Arrange
        var dataService = AddMock<IDataService>();
        var tcs = new TaskCompletionSource<List<Item>>();
        dataService.GetItemsAsync().Returns(tcs.Task);

        var cut = Render<DataGrid>();

        // Initially loading
        cut.Find(".loading").Should().NotBeNull();

        // Act - complete the async operation
        tcs.SetResult(new List<Item> { new("Item 1"), new("Item 2") });

        // Assert - wait for re-render
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".data-row").Count.Should().Be(2);
        });
    }
}
```

---

## Template: EditForm and Validation

```csharp
public sealed class RegistrationFormTests : BlazorTestBase
{
    [Fact]
    public void Should_ShowValidationErrors_When_SubmittedEmpty()
    {
        // Arrange
        var cut = Render<RegistrationForm>();

        // Act - submit without filling fields
        cut.Find("form").Submit();

        // Assert
        cut.FindAll(".validation-message").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Should_ClearErrors_When_ValidInputProvided()
    {
        // Arrange
        var cut = Render<RegistrationForm>();
        cut.Find("form").Submit(); // trigger validation

        // Act - fill in valid data
        cut.Find("#email").Change("user@example.com");
        cut.Find("#password").Change("StrongP@ss1");
        cut.Find("form").Submit();

        // Assert
        cut.FindAll(".validation-message").Count.Should().Be(0);
    }
}
```

---

## Template: Templated Components

```csharp
public sealed class DataListTests : BlazorTestBase
{
    [Fact]
    public void Should_RenderItems_Using_ItemTemplate()
    {
        var items = new List<string> { "Alpha", "Beta", "Gamma" };

        var cut = Render<DataList<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item =>
                $"<li class=\"item\">{item}</li>"));

        cut.FindAll("li.item").Count.Should().Be(3);
    }
}
```

---

## Quick Reference: Finding and Asserting on Elements

```csharp
// Find single element (throws if not found)
var element = cut.Find("button.primary");

// Find all matching elements
var rows = cut.FindAll("tr.data-row");

// Assert on text content
element.TextContent.Should().Be("Click Me");

// Assert on attributes
element.GetAttribute("disabled").Should().NotBeNull();

// Assert on CSS classes
element.ClassList.Should().Contain("active");

// Assert element exists / does not exist
cut.FindAll(".error").Count.Should().Be(0);

// Semantic markup comparison (ignores whitespace, attr order, comments)
cut.MarkupMatches("<p>Expected markup</p>");

// Use diff:ignoreAttributes for partial matching
cut.MarkupMatches(@"<p diff:ignoreAttributes>Expected text</p>");

// Use diff:ignore to skip subtrees
cut.MarkupMatches(@"<div><h1>Title</h1><div diff:ignore></div></div>");
```

---

## Quick Reference: Events and JSInterop

```csharp
// Events — called on elements returned by Find()
element.Click();                         element.DoubleClick();
element.Change("new value");             element.Input("partial");
element.Submit();                        element.KeyDown(Key.Enter);
element.Focus();                         element.Blur();
element.TriggerEvent("oncustomevent", new CustomEventArgs());

// JSInterop
JSInterop.Mode = JSRuntimeMode.Loose;                          // unhandled → default(T)
JSInterop.SetupVoid("myFunc", "arg1");                          // void JS call
JSInterop.Setup<string>("getLocale").SetResult("en-US");        // JS call with return
JSInterop.VerifyInvoke("myFunc", calledTimes: 1);              // verify called
JSInterop.VerifyNotInvoke("unexpectedFunc");                    // verify not called
```

---

## Critical Rules

1. **Register services before rendering** — any `AddMock<T>()` or `Services.AddSingleton()` call must happen before `Render<T>()`
2. **Use semantic comparison** — prefer `MarkupMatches` over string equality; it handles whitespace, attribute order, and self-closing tags correctly
3. **Use `WaitForState` / `WaitForAssertion` for async** — never use `Task.Delay` or `Thread.Sleep` to wait for re-renders
4. **One behavior per test** — follow the same `Should_{Behavior}_When_{Condition}` naming as the `unit-testing` skill
5. **Prefer CSS selectors over index access** — `cut.Find(".error-message")` is more resilient than `cut.FindAll("p")[3]`
6. **Set JSInterop mode intentionally** — use strict mode when you want to verify exact JS calls; use loose mode when JS is incidental to the test
7. **Don't test third-party component internals** — test your component's behavior, not the inner markup of a library component
8. **Fresh context per test** — xUnit creates a new test class instance per test, so each test gets a clean BunitContext

---

## Anti-Patterns to Avoid

```csharp
// ❌ String comparison → ✅ cut.MarkupMatches("<p>Hello</p>")
cut.Markup.Should().Be("<p>Hello</p>");

// ❌ Task.Delay for async → ✅ WaitForAssertion / WaitForState
await Task.Delay(1000);

// ❌ Register services after render → ✅ Register BEFORE Render<T>()
var cut = Render<MyComponent>();
Services.AddSingleton(myService);  // Too late!

// ❌ Testing library internals → ✅ Test YOUR component's behavior
cut.Find(".mudblazor-internal-class");

// ❌ Fragile positional selectors → ✅ Semantic CSS / data-testid
cut.FindAll("div")[4].Find("span");
```

---