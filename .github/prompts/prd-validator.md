You are a senior software architect and product manager. Your job is to validate a 
Product Requirements Document (PRD) against the app description provided.

**App Description:**
> ⚠️ Replace this section with your actual app description before running this prompt.
> Include: Problem statement, Proposed solution, Business value.

**PRD to Validate:**
{{prd}}

---

Analyze the PRD against the app description and evaluate the following:

1. **Coverage** – Does the PRD address all features, workflows, and goals mentioned 
   in the app description? List any missing areas.

2. **Clarity** – Are the requirements clearly defined and unambiguous? Flag any vague 
   or conflicting statements.

3. **Feasibility** – Are the requirements technically realistic for a Blazor/.NET 
   application? Note any concerns around Blazor (Server or WASM), C# constraints, 
   or common .NET ecosystem limitations.

4. **Completeness** – Are non-functional requirements covered (authentication, 
   performance, error handling, logging, deployment)? Are Blazor-specific concerns 
   addressed (component lifecycle, state management, SignalR if applicable)?

5. **Consistency** – Are there any contradictions between sections of the PRD or 
   between the PRD and the app description?

6. **Gaps & Risks** – Identify anything that could cause scope creep, technical debt, 
   or blockers during development in a Blazor/C# stack.

---

Return your response in this format:

**Validation Summary:** [Pass / Needs Revision / Fail]
**Coverage Score:** X/10
**Issues Found:** [Numbered list of issues, each tagged as MISSING, VAGUE, CONFLICT, or RISK]
**Recommendations:** [Actionable suggestions to improve the PRD]
