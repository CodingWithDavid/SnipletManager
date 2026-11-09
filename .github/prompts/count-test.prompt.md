---
mode: agent
description: Check out main, update it, and count xUnit and bUnit tests in the repository
---

Switch to the `main` branch, update it from `origin`, then analyze the entire repository including all folders and subfolders.

Requirements:

1. Check the current git status first and do not overwrite or discard user changes.
2. Check out `main`. If no local `main` exists, create it to track `origin/main`.
3. Fetch and fast-forward `main` from `origin`.
4. Scan the full codebase recursively.
5. Count **xUnit** tests by counting test methods marked with `[Fact]` and `[Theory]`.
6. Count **bUnit** tests by identifying bUnit package references and/or `Bunit` usage. If bUnit is not present, report `0`.
7. Calculate a **grand total** of distinct tests. Because bUnit tests are implemented as xUnit tests, do not add the xUnit and bUnit counts together unless they are proven to be disjoint.
8. Return a concise summary that includes:
   - current branch after update
   - latest commit hash and subject on `main`
   - total xUnit test count
   - total bUnit test count
   - grand total of distinct tests
   - file-by-file breakdown for any test files found

Use Windows-style paths in any file references.
