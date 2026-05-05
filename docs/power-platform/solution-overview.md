# Solution Overview — Bank Shift Exception Management

## Business Problem

NHS Professionals (NHSP) manages the supply of flexible bank workers to NHS trusts across England. When shifts cannot be filled, compliance blockers arise, or urgent staffing requests are raised, these **shift exceptions** must be identified, triaged, and resolved quickly to maintain safe staffing levels.

Today, exception handling relies on manual spreadsheets, emails, and phone calls. This leads to:

- Delayed awareness of unfilled shifts close to start time
- Missed compliance blockers preventing worker deployment
- Inconsistent escalation processes across trusts
- Limited visibility for senior operations managers

## Target Users

| Role | Description |
|---|---|
| **Operations Coordinator** | Front-line user who monitors incoming shift exceptions, triages by priority, and takes initial action (e.g. contacting workers, reassigning shifts). |
| **Compliance Officer** | Reviews exceptions flagged as compliance blockers (expired training, DBS review required) and confirms whether a worker can proceed. |
| **Operations Manager** | Senior user who oversees escalated exceptions, monitors KPIs, and ensures trust SLAs are met. |

## Solution Summary

A Power Platform solution comprising:

| Component | Purpose |
|---|---|
| **Power Apps canvas app** | Single-screen triage interface for operations coordinators to view, filter, and action shift exceptions. |
| **Dataverse** | Structured storage for shifts, workers, and exceptions with relational integrity, audit logging, and role-based security. |
| **Power Automate** | Cloud flows for automated escalation when exceptions remain unresolved past defined thresholds. |
| **GitHub** | Source control for unmanaged solution artefacts, documentation, and ALM pipeline definitions. |

## Architecture Diagram (Conceptual)

```
┌─────────────────┐       ┌───────────────┐     ┌───────────────────┐
│  Canvas App     │────>  │  Dataverse    │<────│  Power Automate   │
│  (Triage UI)    │       │  (Data Store) │     │  (Escalation      │
│                 │       │               │     │   Flows)          │
└─────────────────┘       └───────────────┘     └───────────────────┘
         │                                            │
         ▼                                            ▼
┌──────────────────┐                        ┌───────────────────┐
│  Microsoft Entra │                        │  Outlook / Teams  │
│  ID (AuthN/AuthZ)│                        │  (Notifications)  │
└──────────────────┘                        └───────────────────┘
```

## Key Design Decisions

1. **Canvas app over model-driven app** — A canvas app gives full control over layout and UX, which is important for the 35-minute Power Platform segment of the demo.
2. **Dataverse over SharePoint** — Dataverse provides relational data modelling, row-level security, and audit logging out of the box.
3. **Choice columns over separate lookup tables** — For fixed-list values (Priority, Status, Exception Type), choice columns reduce complexity without sacrificing usability.
4. **Single solution** — All components are packaged in a single Dataverse solution (`nhsp_ShiftExceptionManagement`) for simplified deployment and demo portability.

## Assumptions

| # | Assumption |
|---|---|
| 1 | The demo uses synthetic data only — no real NHS staff or patient data is used. |
| 2 | Authentication is handled via Microsoft Entra ID; all demo users have Power Apps licences. |
| 3 | The live demo can be shown from one Power Platform environment, while the ALM documentation describes how Development, Test, and Demo or Production Pilot environments would be used. |
| 4 | Trusts and wards are represented as text or choice columns rather than full reference data tables. |
| 5 | The canvas app is designed for desktop/tablet use; mobile optimisation is out of scope for the demo. |

## Risks

| # | Risk | Mitigation |
|---|---|---|
| 1 | Demo data does not reflect real-world volumes or complexity. | Clearly communicate that data is illustrative; design supports scaling. |
| 2 | Power Automate flows may not trigger in real time during a live demo. | Pre-test flows; have a manual trigger button as fallback. |
| 3 | Audience unfamiliarity with Power Platform concepts. | Include brief explanations of Dataverse, canvas apps, and flows during the walkthrough. |
| 4 | Licensing constraints may prevent audience from replicating the solution. | Document licence requirements in deployment notes. |
