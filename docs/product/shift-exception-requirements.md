# Bank Shift Exception Management Requirements

## Business Context

NHS Professionals workforce operations teams need a clear way to spot and manage bank shift exceptions before they become operational risks. The demo focuses on coordination work only: unfilled shifts, compliance blockers, urgent staffing requests, escalation status, and audit records.

The solution must avoid patient clinical data and real NHS staff data. All examples use synthetic workforce operations data.

## User Need

Operations coordinators need to see the most urgent exceptions first, understand why each exception matters, and record the action taken. Compliance officers need enough context to resolve compliance blockers without exposing unnecessary personal data. Operations managers need visibility of escalated exceptions and evidence that the process is auditable.

## Scope For Demo

| Area | Included | Out Of Scope |
| --- | --- | --- |
| Exception triage | View, filter, prioritise, and update shift exceptions | Clinical risk assessment |
| Workforce data | Synthetic workers, shifts, compliance status, availability | Real staff records or payroll data |
| Escalation | Coordinator notification, trust contact escalation, audit action | Live integration with external rostering systems |
| App experience | Power Apps canvas app design and Power Fx formulas | Full production build in Power Apps Studio |
| ALM | GitHub documentation, prompts, review, deployment strategy | Complete CI/CD pipeline implementation |

## Personas

| Persona | Need | Success Measure |
| --- | --- | --- |
| Operations Coordinator | Quickly identify exceptions requiring action | Can filter to Critical and High exceptions starting soon |
| Compliance Officer | Review blockers without unnecessary personal data | Can see compliance status and update operational notes |
| Operations Manager | Monitor escalation and audit readiness | Can see unresolved escalations and action history |
| Product Manager | Convert the scenario into backlog items | GitHub Issues cover user value and acceptance criteria |

## Functional Requirements

| ID | Requirement | Priority | Acceptance Criteria |
| --- | --- | --- | --- |
| REQ-001 | Show open and in-progress shift exceptions in a dashboard | Must | Coordinator can see exception ID, type, priority, status, trust, ward, and shift start time |
| REQ-002 | Filter exceptions by Trust, Ward, Priority, Status, and Exception Type | Must | Selecting any filter narrows the dashboard list and an All option clears the filter |
| REQ-003 | Highlight exceptions for shifts starting in the next 24 hours | Must | Time-critical exceptions can be shown in a dedicated view or urgent filter |
| REQ-004 | Calculate a transparent operational risk score | Should | Risk score uses priority, exception type, start time, staffing gap, and escalation status |
| REQ-005 | Validate worker assignment readiness | Should | App checks availability, compliance, role match, and preferred Trust before assignment |
| REQ-006 | Save exception actions and resolution notes | Must | Coordinator can update status and notes, with error handling if save fails |
| REQ-007 | Escalate Critical and High exceptions close to shift start | Must | Flow notifies coordinator and escalates unresolved exceptions after a configured delay |
| REQ-008 | Record audit actions | Must | Exception Action records are created for notifications, escalations, skipped actions, and flow failures |
| REQ-009 | Restrict access by role | Must | Dataverse roles separate coordinator, compliance officer, and manager permissions |
| REQ-010 | Provide accessible screen and notification designs | Must | Controls have labels, status is not shown by colour alone, and keyboard navigation is considered |

## Non-Functional Requirements

| Area | Requirement |
| --- | --- |
| Security | Use Microsoft Entra ID, Dataverse security roles, least privilege, and service-owned flow connections |
| Privacy | Do not include patient data, clinical context, or real staff personal data in repo or notifications |
| Auditability | Enable Dataverse auditing for key exception fields and create Exception Action records |
| Accessibility | Meet WCAG 2.1 AA intent for contrast, labels, keyboard navigation, and readable notification content |
| Maintainability | Keep Power Fx formulas readable and document control/table assumptions |
| Demo reliability | Use synthetic data, pre-tested prompts, and a fallback runbook if a live service is unavailable |

## Assumptions

| # | Assumption |
| --- | --- |
| 1 | The demo uses fictional trusts, wards, workers, shifts, and contact addresses. |
| 2 | The app is a Power Apps canvas app backed by Dataverse. |
| 3 | Power Automate handles escalation notifications and audit action records. |
| 4 | GitHub Issues are created in the first 10 minutes for the delivery backlog. |
| 5 | The remaining 35 minutes show GitHub Copilot supporting Power Platform delivery artefacts. |
| 6 | The demo explains Power Platform concepts in simple terms for mixed technical and product audiences. |

## Risks

| Risk | Mitigation |
| --- | --- |
| Live prompts produce too much output for the timebox | Use the prompt sequence and generate one artefact at a time |
| Audience expects a fully built app rather than delivery artefacts | State that this is a Copilot-supported delivery demo, not a production implementation demo |
| Sample data appears too close to real data | Reiterate that all records are synthetic and avoid real NHS staff or patient references |
| Flow delay is impractical in a live session | Use short demo delays through environment variables or show the flow design and audit records |
| Security questions require more detail | Use the security model and ALM docs as prepared reference material |

## Test Cases

| # | Scenario | Expected Result |
| --- | --- | --- |
| 1 | Filter dashboard to Critical exceptions | Only Critical exceptions remain visible |
| 2 | Select a High priority exception starting within 24 hours | Detail panel shows shift context and escalation status |
| 3 | Attempt to assign a worker with expired training | Assignment is blocked and a clear message is shown |
| 4 | Save a resolution note | Exception status and notes are saved or a friendly error appears |
| 5 | Escalation flow handles unresolved Critical exception | Coordinator notification, trust escalation, and Exception Action records are produced |
