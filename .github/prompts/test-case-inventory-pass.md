# INVENTORY PASS — Codebase Feature & Architecture Mapping

You are a senior software architect and QA analyst.

Your task is to scan this entire repository (opened in VS Code) and produce a structured inventory of the system so that comprehensive test cases can be generated in a second pass.

Do NOT generate test cases yet.
Do NOT assume behavior that is not visible in code.
Do NOT summarize vaguely — be concrete and reference file paths.

---

## OBJECTIVE

Produce a detailed system inventory including:

1. Functional Areas / Feature Modules
2. User Roles & Authorization Rules
3. Pages / UI Screens (with routes)
4. API Endpoints (with HTTP verbs and routes)
5. Services / Business Logic Classes
6. Data Entities / Models
7. Validation Rules
8. State Machines / Workflow Transitions
9. External Integrations
10. High-Risk Logic Areas (money, permissions, state transitions, deletes, etc.)

---

## REQUIRED OUTPUT FORMAT

Return your output in the following structured format:

---

# System Inventory

## 1. Application Overview
- Application Type (Blazor Server, Web API, MVC, etc.)
- Primary Entry Points
- Authentication Mechanism
- Authorization Mechanism

---

## 2. User Roles & Permissions

For each role found:
- Role Name
- Where Defined (file path)
- Where Enforced (Authorize attributes, policy checks, UI visibility logic)
- Notable Restrictions

---

## 3. Feature Areas

For each feature area:

### Feature: <Name>

**Purpose**
Short functional description based strictly on code.

**UI Pages**
- Route:
- Component/Page File:
- Related Components:

**API Endpoints**
- HTTP Verb + Route:
- Controller File:
- Method Name:

**Business Logic**
- Service Classes:
- Key Methods:
- Validation Rules:
- Workflow Rules:

**Data Layer**
- Entities:
- DbSets:
- Migrations (if relevant):

**State Transitions (if applicable)**
- From → To
- Conditions

**Risk Notes**
- Security-sensitive?
- Financial impact?
- Destructive operations?
- Concurrency risk?

---

## 4. Cross-Cutting Concerns

- Logging
- Error Handling
- Middleware
- Feature Flags
- Background Jobs
- Caching
- File Upload/Download
- Reporting

---

## 5. Identified High-Risk Areas for Testing

List areas that:
- Modify state
- Enforce permissions
- Handle money
- Perform deletes
- Perform imports/exports
- Rely on concurrency
- Have complex validation logic

---

## RULES

- Include file paths for everything.
- Do NOT guess.
- If something appears incomplete, label it: (Needs Confirmation).
- Prefer precision over verbosity.
- Do not generate test cases in this pass.

When finished, stop. This output will be used as input for a second prompt to generate Azure DevOps test cases.