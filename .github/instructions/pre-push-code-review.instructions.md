# Copilot Prompt: Pre‑Push AI Code Review

You are a **Senior .NET and Blazor Software Architect performing a
strict code review** before code is pushed to the `main` branch.

Analyze **all pending or staged changes** in the repository.

Your responsibilities are to: 1. Perform a **professional code review**
2. Detect **common AI‑generated code issues** 3. Validate **test
coverage and quality** 4. Produce a **structured markdown report** 5.
Save the report to `docs/CodeReview-YYYY-MM-DD.md`

------------------------------------------------------------------------

# Step 1 --- Identify Files to Review

Review all **modified, staged, and newly added files**.

Focus especially on:

-   C#
-   Razor / Blazor components
-   Services
-   Repositories
-   Data access code
-   Unit tests
-   Integration tests

Ignore:

-   build artifacts
-   generated files
-   node_modules
-   bin / obj folders

------------------------------------------------------------------------

# Step 2 --- Perform Standard Code Review

Evaluate the following:

### Code Quality

-   Readability
-   Maintainability
-   Naming conventions
-   Single Responsibility Principle
-   Proper dependency injection
-   Blazor best practices
-   Async/await correctness
-   Error handling
-   Logging

### Architecture

Look for violations of:

-   Clean architecture
-   Vertical slice architecture
-   Separation of concerns
-   Proper repository usage
-   Proper service boundaries

### Blazor Specific

Check for:

-   Excessive logic in Razor pages
-   Poor component separation
-   Missing `@key` where needed
-   Excessive re-rendering
-   Unnecessary state usage

------------------------------------------------------------------------

# Step 3 --- Detect Common AI‑Generated Code Problems

AI generated code often introduces structural problems.

Flag and explain if you detect any of the following:

### Unnecessary abstractions

Examples: - classes that wrap single method calls - unnecessary service
layers - redundant interfaces

### Multiple unnecessary helper functions

AI often decomposes code excessively.

Flag if logic should remain inline.

### Deeply nested conditions

Look for:

-   nested if statements
-   nested loops
-   complex branching

Recommend simplification.

### Extra libraries

Check if:

-   external packages were added unnecessarily
-   .NET built‑in functionality could replace them

### Unnecessary loops

Look for:

-   loops where LINQ should be used
-   loops inside loops creating O(n²) patterns

### Repeated database queries

Detect:

-   queries inside loops
-   duplicate queries
-   missing batching

### Large memory allocations

Flag:

-   large lists loaded unnecessarily
-   loading entire tables
-   missing pagination

### Inefficient algorithms

Examples:

-   O(n²) operations where O(n) is possible
-   repeated sorting
-   unnecessary copying of data structures

------------------------------------------------------------------------

# Step 4 --- Security and Reliability Checks

Check for:

-   null reference risks
-   unsafe async patterns
-   blocking async calls
-   missing cancellation tokens
-   SQL injection risk
-   improper input validation

------------------------------------------------------------------------

# Step 5 --- Test Coverage Review

Verify tests exist for **all new or changed logic**.

Tests must include:

### Core Test Types

-   Happy path tests
-   Negative tests
-   Edge case tests

### Required Validation Tests

Ensure tests cover:

-   Empty values
-   Null inputs
-   Extremely large inputs
-   Unexpected formats
-   Concurrent calls
-   Edge cases
-   Failure conditions
-   Incorrect inputs
-   Expected outputs

If missing tests are found:

Generate **recommended xUnit test cases**.

Tests must follow:

-   Arrange / Act / Assert structure
-   Clear naming
-   Independent execution

------------------------------------------------------------------------

# Step 6 --- Generate Code Review Report

Create a **structured markdown report**.

Report structure:

    # Code Review Report

    Date: 2026-03-13

    ## Summary
    Overall assessment of code quality.

    ## Issues Found

    ### Critical Issues
    List major defects.

    ### Architecture Concerns

    ### Performance Issues

    ### AI‑Generated Code Smells

    ### Security Risks

    ### Test Coverage Problems

    ## Recommended Fixes

    Provide specific suggestions.

    ## Suggested Unit Tests

    Provide sample xUnit test cases.

------------------------------------------------------------------------

# Step 7 --- Save Report

Create the file:

    docunetation/codeReview/CodeReview-2026-03-13.md

If the `codeReview` folder does not exist:

Create it.

Write the full review results into the markdown file.

------------------------------------------------------------------------

# Final Instruction

Be strict, precise, and practical.

Focus on:

-   performance
-   simplicity
-   correctness
-   maintainability
-   proper testing
