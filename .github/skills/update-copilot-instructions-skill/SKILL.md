# Update Copilot Instructions Retrospective Skill

Use this skill after completing a work session when the user asks to reflect on the work and update `copilotinstrucitons.md` with lessons learned.

## Purpose

After finishing a task, perform a short retrospective, identify reusable context and process improvements, then append those learnings to `copilotinstrucitons.md` without removing or overwriting existing content.

## Trigger

Run this skill when the user says something like:

- "Update copilotinstrucitons.md with what you learned."
- "Add this lesson to my Copilot instructions."
- "Reflect on the work and update the instructions file."
- "Run the retrospective skill."

## Inputs Needed

Before updating the file, use the completed conversation or work session as the source of truth. If the file path is not obvious, look for `copilotinstrucitons.md` in the current repository or workspace.

Do not ask for clarification unless the file cannot be found or there are multiple plausible target files.

## Required Retrospective

First, briefly answer these questions for yourself based on the completed task:

1. What assumptions or detours slowed the work down?
2. What information, context, constraints, examples, or framing would have allowed a faster or more accurate answer if provided upfront?
3. In hindsight, what was the most direct path from the original request to the final answer?

Use those answers to decide what should be appended to the instructions file.

## File Update Rules

Update `copilotinstrucitons.md` using append-only behavior.

- Do not remove existing entries.
- Do not overwrite existing entries.
- Do not reorganize unrelated content.
- Preserve the user's wording where practical.
- If a required section already exists, append to it.
- If a required section does not exist, create it at the end of the file.
- Keep entries concise and reusable.
- Avoid adding one-off details that will not help future work.

## Required Sections and Entries

Ensure the file contains these sections. Append the corresponding information under each one.

### About me / my work

Add any newly discovered durable context, constraints, preferences, project conventions, naming quirks, workflow details, or recurring expectations that would help start faster next time.

Examples:

- The user prefers append-only changes to instruction files unless explicitly asked otherwise.
- The user wants reusable prompt templates captured after successful workflows.
- The project uses `copilotinstrucitons.md` as the persistent instruction file, including the misspelled filename.

### Prompt templates

Add the ideal prompt that would have produced the final outcome most directly, labelled by task type.

Format:

```markdown
#### Task type: <short task label>

<ideal prompt text>
```

The ideal prompt should include:

- The actual goal.
- The important constraints.
- The expected output format.
- Any repository, file, framework, or workflow context that prevented detours.

### Working patterns

Add one brief lesson learned entry. It should be one sentence describing the shortcut or direct path that was available but not taken.

Format:

```markdown
- Lesson learned: <one concise sentence>
```

## Suggested Workflow

1. Locate `copilotinstrucitons.md`.
2. Read the current contents.
3. Identify whether the required sections already exist.
4. Draft the retrospective findings.
5. Append new entries to the relevant sections.
6. Verify that no existing content was removed or overwritten.
7. Summarize what was added.

## Quality Bar

A good update should be:

- Short enough to stay useful.
- Specific enough to prevent the same detour next time.
- General enough to apply beyond the single task.
- Clearly append-only.
- Easy for Copilot or another assistant to follow.

## Example Append

```markdown
## About me / my work

- The user wants post-task retrospectives to be captured in `copilotinstrucitons.md` using append-only updates.
- The user prefers reusable lessons, constraints, and prompt templates over long narratives.

## Prompt templates

#### Task type: Post-task retrospective instruction update

After completing the task, reflect on what slowed the work down, what context would have helped upfront, and the most direct path in hindsight. Then update `copilotinstrucitons.md` append-only by adding durable context to "About me / my work", the ideal prompt to "Prompt templates", and a one-sentence shortcut under "Working patterns". Do not remove, overwrite, or reorganize existing entries.

## Working patterns

- Lesson learned: The fastest path was to treat the retrospective as structured reusable instruction updates instead of writing a long narrative summary first.
```
