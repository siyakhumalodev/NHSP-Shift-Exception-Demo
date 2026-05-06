# NHSP - Escalate Shift Exception

## Business context

This cloud flow monitors synthetic shift exception records and escalates unresolved high-risk workforce exceptions for the NHSP Shift Exception Manager demo. It supports operational workforce coordination only and must not include patient data, clinical context, real NHS staff data, or real escalation contacts.

## User need

Operations coordinators need Critical and High priority exceptions for near-start shifts to be visible quickly, with a traceable audit trail showing notification, recheck, escalation, skip, and failure outcomes.

## Technical approach

`clientdata.json` contains the Power Automate Modern Flow definition used to create a Dataverse `workflow` row through the deployment helper.

The flow:

- Triggers when a `nhsp_ShiftException` row is added or modified.
- Filters to Critical or High priority records that are not Resolved, Closed, or already Escalated.
- Retrieves the related `nhsp_Shift` and exception owner.
- Checks that the shift starts within 24 hours.
- Notifies the coordinator through Office 365 Outlook.
- Writes `nhsp_ExceptionAction` audit rows.
- Marks the exception as Pending Trust Response.
- Waits 5 minutes for Critical or 10 minutes for High in the demo environment.
- Rechecks the exception status.
- Escalates to a synthetic trust contact if unresolved.
- Writes failure audit records if the main scope fails.

## Deployment

Run from the repository root:

```powershell
.\tools\deployment\deploy-power-automate-flow.ps1
```

The helper creates or updates the flow as a draft/off Modern Flow in the `nhspshiftexceptiondemo` solution. After deployment, open the flow in Power Automate, bind the Dataverse and Office 365 Outlook connections, confirm recipients are demo-safe, then turn it on for testing.

## Security considerations

- Use service-owned Dataverse and Outlook connections for test or demo environments.
- Keep `nhsp_EnableExternalNotifications` false unless explicit approval has been given.
- Use only synthetic trust contact rows where `nhsp_IsDemoContact = Yes`.
- Keep notification content to operational workforce context: exception ID, priority, ward, start time, and fill status.
- Write all automated outcomes to `nhsp_ExceptionAction` for auditability.

## Accessibility considerations

- Email subjects are descriptive and high signal.
- Message bodies use structured labels instead of colour-only status cues.
- Notification text is concise enough for screen readers and mobile previews.

## Test cases

| Test | Expected result |
| --- | --- |
| Create a Critical open exception for a shift starting within 24 hours | Coordinator email is sent, an audit action is created, and escalation status becomes Pending Trust Response. |
| Create a High open exception for a shift starting within 24 hours | Coordinator email is sent and a 10-minute demo delay starts. |
| Create a Critical exception for a shift starting after 24 hours | No notification is sent and a Skipped audit action is created. |
| Resolve an exception during the delay | Trust escalation is skipped and a Skipped audit action is created. |
| Leave a Critical exception unresolved after the delay | A synthetic trust contact notification is sent, an audit action is created, and escalation status becomes Escalated. |
| Remove matching synthetic trust contact routing | A Flow Error audit action is created and no external notification is sent. |

## Assumptions

- The Dataverse schema has been deployed with `nhsp_Shift`, `nhsp_ShiftException`, `nhsp_ExceptionAction`, and `nhsp_TrustContact` tables.
- Synthetic sample data has been imported before functional testing.
- The current environment has valid Dataverse and Office 365 Outlook connector connections.
- The flow is reviewed before being turned on because connection references are environment-specific.

## Risks

| Risk | Mitigation |
| --- | --- |
| Repeated updates trigger duplicate notifications | Trigger filter excludes Resolved, Closed, and already Escalated records. |
| Delayed run continues after manual resolution | The flow re-reads the exception before trust escalation. |
| Real contacts are accidentally notified | The trust-contact query requires `nhsp_IsDemoContact = Yes` and demo-safe contact data. |
| Connection binding is missing after deployment | The flow is created as draft/off and must be reviewed before activation. |
