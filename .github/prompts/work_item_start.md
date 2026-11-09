---
model: gpt-5.4
description: Fetches the latest code and creates a new branch from a work item name.
---

Start a new work item branch using this input: `{{input}}`

Your job is to sync the local repository with source control and create a new branch for the work item.

## Rules

- Do **not** edit source files, create commits, or push anything.
- If the working tree has staged, unstaged, or untracked changes, stop and report the issue. Do **not** stash, discard, or overwrite anything.
- Prefer `git switch` over `git checkout`.
- Use the repository default branch if it can be determined from `origin/HEAD`; otherwise use `Development`.
- Fetch from `origin` before creating the branch.
- If the current branch is not the default branch, switch to the default branch before pulling and creating the new branch.
- Update the default branch from remote using a non-destructive fast-forward-only pull.
- Create and switch to a new branch based on `{{input}}`.
- If `{{input}}` contains spaces or characters that are invalid in Git branch names, normalize it to a git-safe name by:
  1. trimming whitespace
  2. converting to lowercase
  3. replacing spaces with `-`
  4. removing invalid Git branch characters
- If the target branch already exists locally or on `origin`, stop and report that instead of reusing it.

## Process

1. Validate that `{{input}}` was provided.
2. Check that the working tree is clean.
3. Determine the default branch from `origin/HEAD` if available.
4. Run `git fetch origin --prune`.
5. Check the current branch.
6. If the current branch is not the default branch, switch to the default branch.
7. Run `git pull --ff-only origin <default-branch>`.
8. Create and switch to the new branch derived from `{{input}}`.
9. Report the final branch name and the branch it was created from.

## Output

Return:

- the default branch used
- the final branch name created
- whether the repo was updated successfully
- the current branch after creation
