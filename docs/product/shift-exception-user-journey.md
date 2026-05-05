# Shift Exception User Journey

## Business Context

The journey shows how a workforce operations team identifies, triages, and escalates bank shift exceptions. It is written to support the NHS Professionals demo and to give GitHub Copilot enough product context before generating Power Platform design artefacts.

## Primary Journey: Triage A High-Risk Exception

| Step | User | Action | System Response | Evidence For Demo |
| --- | --- | --- | --- | --- |
| 1 | Operations Coordinator | Opens the Shift Exception Dashboard | Canvas app shows open and in-progress exceptions sorted by urgency | Dashboard screen design and gallery formula |
| 2 | Operations Coordinator | Filters by Critical or High priority | List narrows to exceptions needing immediate attention | Power Fx filter formula |
| 3 | Operations Coordinator | Selects an exception for a shift starting soon | Detail panel shows related shift, ward, role, staffing gap, and escalation status | Dataverse lookup from Shift Exception to Shift |
| 4 | Operations Coordinator | Reviews possible workers | Worker Availability screen shows availability and compliance status | Worker table and validation formula |
| 5 | Operations Coordinator | Attempts to assign or progress an action | App validates worker readiness and records notes | Power Fx validation and save handling |
| 6 | Operations Coordinator | Leaves a Critical exception unresolved | Power Automate identifies priority and timing | Flow trigger and conditions |
| 7 | Power Automate | Sends coordinator notification | Notification includes operational shift context only | Flow notification content |
| 8 | Power Automate | Rechecks after configured delay | If unresolved, trust contact escalation is sent | Flow delay and recheck logic |
| 9 | Operations Manager | Reviews escalation status and audit trail | Exception Action records and Dataverse audit show what happened | Audit table and security model |

## Supporting Journey: Compliance Blocker

| Step | User | Action | System Response |
| --- | --- | --- | --- |
| 1 | Compliance Officer | Filters exceptions to Compliance Blocker | App shows blockers requiring compliance review |
| 2 | Compliance Officer | Reviews worker compliance status | App shows operational status without unnecessary personal detail |
| 3 | Compliance Officer | Updates notes and status | Dataverse records the update and audit captures key fields |
| 4 | Operations Coordinator | Refreshes dashboard | Exception moves from Open to In Progress or Resolved as appropriate |

## Data Used In The Demo

| Sample File | Used For |
| --- | --- |
| `samples/sample-shifts.csv` | Shift context, start times, trust, ward, role, and fill status |
| `samples/sample-workers.csv` | Worker availability and compliance validation |
| `samples/sample-exceptions.csv` | Dashboard and escalation scenarios |
| `samples/sample-exception-actions.csv` | Audit trail examples for notifications, escalations, and flow outcomes |
| `samples/sample-trust-contacts.csv` | Synthetic escalation routing without real contact details |

## Accessibility Considerations

| Need | Design Response |
| --- | --- |
| Screen reader users need meaningful context | Gallery rows and buttons use descriptive accessible labels |
| Users should not rely on colour alone | Priority and compliance states use text plus icons or labels |
| Keyboard users need predictable navigation | Tab order follows filters, list, detail, and action buttons |
| Users need clear feedback | Empty states, validation messages, and save errors are written in plain language |

## Security And Auditability Considerations

| Consideration | Design Response |
| --- | --- |
| Least privilege | Dataverse roles separate coordinator, compliance officer, and manager actions |
| Staff data minimisation | Notifications include shift and exception context, not unnecessary worker details |
| No patient data | The scenario is operational workforce management only |
| Audit trail | Dataverse auditing and Exception Action records capture key decisions and flow actions |
| Environment safety | Escalation contacts are synthetic in demo and configured through environment variables or a small table |

## Demo Notes

- Start the Power Platform segment by showing this journey, then ask Copilot to generate or refine artefacts from the repository context.
- Use `EXC-3002` as the strongest escalation example because it is Critical and already marked as Escalated in the sample data.
- Use `EXC-3001` as a practical unfilled shift scenario for filtering, detail review, and coordinator action.
- Keep all spoken examples operational. Avoid clinical scenarios, patient impact claims, or real staff references.

## Assumptions And Risks

| Area | Detail |
| --- | --- |
| Assumption | The user journey is a demo-ready target workflow, not a confirmed production process for NHS Professionals. |
| Assumption | Trust and ward names are fictional and used only for demonstration. |
| Risk | Live Power Automate delays are not practical in a 35-minute segment. Use short demo variables or pre-created audit examples. |
| Risk | Product and technical audiences may focus on different concerns. Use the journey to connect user value to implementation artefacts. |
