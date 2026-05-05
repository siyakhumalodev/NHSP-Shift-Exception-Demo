# Power Automate Flow Design

## Overview

This document defines a Power Automate cloud flow for escalating high-risk shift exceptions in the **NHSP Shift Exception Manager** demo. The flow monitors Dataverse Shift Exception records, identifies urgent Critical or High priority exceptions, notifies the operations coordinator, escalates unresolved exceptions to the relevant trust contact, and records each action for auditability.

## Business Context

Operations coordinators need a reliable way to identify shift risks that may affect workforce cover. The flow supports operational escalation only. It does not use patient clinical data, real NHS staff data, or clinical decision-making logic.

## User Need

As an operations coordinator, I need urgent shift exceptions to be flagged and escalated automatically when a shift is close to starting, so unresolved workforce risks are visible to the right people in time for action.

## Technical Approach

Use a Dataverse-triggered automated cloud flow:

- Trigger when a `nhsp_ShiftException` row is created or updated.
- Retrieve the related `nhsp_Shift` row.
- Continue only when Priority is Critical or High and the shift starts within 24 hours.
- Notify the assigned operations coordinator.
- Wait for a defined response window, then re-check the exception status.
- Escalate to the trust contact if the exception is still unresolved.
- Write audit records to an `nhsp_ExceptionAction` table.
- Use scoped error handling and Power Automate retry policies.

---

## Dataverse Tables

### Existing Tables

| Display Name | Logical Name | Use In Flow |
| --- | --- | --- |
| Shift Exception | `nhsp_ShiftException` | Trigger record and escalation status update |
| Shift | `nhsp_Shift` | Shift start time, Trust, Ward, Role, and staffing gap |
| Worker | `nhsp_Worker` | Not directly updated by this flow |

### Proposed Audit Table: Exception Action

The prompt requires an audit record to be written to **Exception Action**. This table is a recommended addition to the Dataverse model.

| Column | Display Name | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| `nhsp_ExceptionActionId` | Exception Action ID | Text (Primary Name) | Yes | Business key, e.g. `ACT-4001` |
| `nhsp_ShiftException` | Shift Exception | Lookup to `nhsp_ShiftException` | Yes | Exception this action relates to |
| `nhsp_ActionType` | Action Type | Choice | Yes | Coordinator Notification, Trust Escalation, Flow Error |
| `nhsp_ActionStatus` | Action Status | Choice | Yes | Sent, Skipped, Failed, Retried |
| `nhsp_ActionTime` | Action Time | Date and Time | Yes | Time the flow recorded the action |
| `nhsp_ActionBy` | Action By | Text | Yes | Flow service account or user display name |
| `nhsp_Notes` | Notes | Multiline Text | No | Operational message summary, no patient data |

---

## Flow Summary

| Item | Design |
| --- | --- |
| Flow name | `NHSP - Escalate Shift Exception` |
| Flow type | Automated cloud flow |
| Trigger connector | Microsoft Dataverse |
| Trigger table | `nhsp_ShiftException` |
| Trigger event | Row is added, modified, or deleted: Added or Modified only |
| Scope | Organisation |
| Run as | Dedicated service account or connection reference in managed solution |
| Primary owner | NHSP Operations Manager |

---

## Trigger

### Dataverse Trigger

Use the Dataverse trigger **When a row is added, modified or deleted**.

| Setting | Value |
| --- | --- |
| Change type | Added or Modified |
| Table name | Shift Exceptions |
| Scope | Organisation |
| Select columns | `nhsp_priority,nhsp_status,nhsp_escalationstatus,nhsp_shift,nhsp_owner,nhsp_resolutionnotes,nhsp_resolvedtime` |
| Filter rows | `nhsp_status ne 'Resolved' and nhsp_status ne 'Closed'` |

**Trigger condition:**

Use a trigger condition to reduce unnecessary flow runs. Adjust choice values to the environment's generated numeric option set values if required.

```text
@or(
  equals(triggerOutputs()?['body/nhsp_priority@OData.Community.Display.V1.FormattedValue'], 'Critical'),
  equals(triggerOutputs()?['body/nhsp_priority@OData.Community.Display.V1.FormattedValue'], 'High')
)
```

**Assumptions:**

- The formatted value is available in the trigger output. If not, use the numeric Dataverse choice values.
- The flow runs under a service account with read and update permissions for Shift Exception and create permissions for Exception Action.
- Resolved and Closed exceptions should not start a new escalation path.

---

## Flow Steps

### Scope: Main Processing

| Step | Action | Configuration | Purpose |
| --- | --- | --- | --- |
| 1 | Initialize variable | `varFlowRunTime = utcNow()` | Consistent timestamp for audit notes |
| 2 | Initialize variable | `varFlowActor = 'Power Automate - NHSP Escalation Flow'` | Identifies the automated actor |
| 3 | Get a row by ID | Table: `nhsp_Shift`, Row ID: Shift lookup from trigger | Retrieves related shift details |
| 4 | Compose | `ShiftStartUtc` from `nhsp_StartTime` | Normalises shift start time for conditions |
| 5 | Condition | Priority is Critical or High | Confirms high-risk priority |
| 6 | Condition | Shift starts within next 24 hours | Confirms time-critical exception |
| 7 | Get a row by ID | Owner system user from `nhsp_Owner` | Gets coordinator contact details |
| 8 | Send notification | Teams or email to coordinator | Alerts the responsible coordinator |
| 9 | Add a new row | `nhsp_ExceptionAction` | Records coordinator notification |
| 10 | Update a row | `nhsp_ShiftException.nhsp_EscalationStatus = Pending Trust Response` | Marks exception as in escalation monitoring |
| 11 | Delay | 2 hours for Critical, 4 hours for High | Allows coordinator action before external escalation |
| 12 | Get a row by ID | Re-read `nhsp_ShiftException` | Checks latest status after delay |
| 13 | Condition | Status is not Resolved or Closed | Determines if trust escalation is needed |
| 14 | Send notification | Email to trust contact | Escalates unresolved exception |
| 15 | Add a new row | `nhsp_ExceptionAction` | Records trust escalation |
| 16 | Update a row | `nhsp_ShiftException.nhsp_EscalationStatus = Escalated` | Updates exception escalation state |

---

## Conditions

### 1. Priority Is Critical Or High

**Expression:**

```text
@or(
  equals(triggerOutputs()?['body/nhsp_priority@OData.Community.Display.V1.FormattedValue'], 'Critical'),
  equals(triggerOutputs()?['body/nhsp_priority@OData.Community.Display.V1.FormattedValue'], 'High')
)
```

**If yes:** Continue to shift timing check.

**If no:** Terminate with status `Succeeded` and reason `Priority does not require escalation`.

### 2. Shift Starts Within Next 24 Hours

**Expression:**

```text
@and(
  greaterOrEquals(outputs('Get_related_shift')?['body/nhsp_starttime'], utcNow()),
  lessOrEquals(outputs('Get_related_shift')?['body/nhsp_starttime'], addHours(utcNow(), 24))
)
```

**If yes:** Notify the coordinator.

**If no:** Add an Exception Action record with Action Status `Skipped`, then terminate successfully.

### 3. Exception Remains Unresolved After Delay

**Expression:**

```text
@not(
  or(
    equals(outputs('Recheck_shift_exception')?['body/nhsp_status@OData.Community.Display.V1.FormattedValue'], 'Resolved'),
    equals(outputs('Recheck_shift_exception')?['body/nhsp_status@OData.Community.Display.V1.FormattedValue'], 'Closed')
  )
)
```

**If yes:** Escalate to the trust contact.

**If no:** Add an Exception Action record with Action Type `Coordinator Notification` and Action Status `Skipped`, noting that no trust escalation was needed.

---

## Dataverse Actions

### Get Related Shift

| Field | Value |
| --- | --- |
| Action | Dataverse: Get a row by ID |
| Table | `nhsp_Shift` |
| Row ID | Shift lookup from trigger row |
| Columns | `nhsp_shiftid,nhsp_trust,nhsp_ward,nhsp_role,nhsp_starttime,nhsp_endtime,nhsp_requiredworkers,nhsp_filledworkers,nhsp_status` |

### Update Shift Exception To Pending Trust Response

| Field | Value |
| --- | --- |
| Action | Dataverse: Update a row |
| Table | `nhsp_ShiftException` |
| Row ID | Trigger row ID |
| Column update | `nhsp_EscalationStatus = Pending Trust Response` |

### Update Shift Exception To Escalated

| Field | Value |
| --- | --- |
| Action | Dataverse: Update a row |
| Table | `nhsp_ShiftException` |
| Row ID | Trigger row ID |
| Column update | `nhsp_EscalationStatus = Escalated` |

### Create Exception Action Audit Record

| Field | Example Value |
| --- | --- |
| `nhsp_ExceptionActionId` | `concat('ACT-', formatDateTime(utcNow(), 'yyyyMMddHHmmss'))` |
| `nhsp_ShiftException` | Trigger row lookup |
| `nhsp_ActionType` | `Coordinator Notification`, `Trust Escalation`, or `Flow Error` |
| `nhsp_ActionStatus` | `Sent`, `Skipped`, `Failed`, or `Retried` |
| `nhsp_ActionTime` | `utcNow()` |
| `nhsp_ActionBy` | `Power Automate - NHSP Escalation Flow` |
| `nhsp_Notes` | Operational summary of the action taken |

---

## Notification Content

### Operations Coordinator Notification

**Channel:** Microsoft Teams message or Outlook email to the exception owner.

**Subject:**

```text
Urgent shift exception: @{triggerOutputs()?['body/nhsp_exceptionid']}
```

**Body:**

```text
A high-priority shift exception needs review.

Exception ID: @{triggerOutputs()?['body/nhsp_exceptionid']}
Priority: @{triggerOutputs()?['body/nhsp_priority@OData.Community.Display.V1.FormattedValue']}
Type: @{triggerOutputs()?['body/nhsp_exceptiontype@OData.Community.Display.V1.FormattedValue']}
Trust: @{outputs('Get_related_shift')?['body/nhsp_trust@OData.Community.Display.V1.FormattedValue']}
Ward: @{outputs('Get_related_shift')?['body/nhsp_ward']}
Role: @{outputs('Get_related_shift')?['body/nhsp_role@OData.Community.Display.V1.FormattedValue']}
Shift start: @{outputs('Get_related_shift')?['body/nhsp_starttime']}
Fill status: @{outputs('Get_related_shift')?['body/nhsp_filledworkers']} of @{outputs('Get_related_shift')?['body/nhsp_requiredworkers']} filled

Please review the exception in the NHSP Shift Exception Manager app and update the status or resolution notes.
```

### Trust Contact Escalation Notification

**Channel:** Outlook email to a configured trust contact.

For the demo, store trust contact routing in environment variables or a small Dataverse configuration table such as `nhsp_TrustContact`. Avoid hard-coding real contact addresses.

**Subject:**

```text
Escalation required: unresolved shift exception @{triggerOutputs()?['body/nhsp_exceptionid']}
```

**Body:**

```text
An urgent shift exception remains unresolved and requires trust review.

Exception ID: @{triggerOutputs()?['body/nhsp_exceptionid']}
Priority: @{triggerOutputs()?['body/nhsp_priority@OData.Community.Display.V1.FormattedValue']}
Status: @{outputs('Recheck_shift_exception')?['body/nhsp_status@OData.Community.Display.V1.FormattedValue']}
Trust: @{outputs('Get_related_shift')?['body/nhsp_trust@OData.Community.Display.V1.FormattedValue']}
Ward: @{outputs('Get_related_shift')?['body/nhsp_ward']}
Role: @{outputs('Get_related_shift')?['body/nhsp_role@OData.Community.Display.V1.FormattedValue']}
Shift start: @{outputs('Get_related_shift')?['body/nhsp_starttime']}

Please review available workforce cover options and respond through the agreed operational escalation route.
```

**Content rules:**

- Do not include patient details or clinical context.
- Do not include unnecessary worker personal data.
- Include enough shift context for operational triage: Trust, Ward, Role, start time, exception type, and priority.

---

## Error Handling And Retries

### Scope Structure

Use three top-level scopes:

| Scope | Run After | Purpose |
| --- | --- | --- |
| `Scope - Main Processing` | Default | Normal flow path |
| `Scope - Error Handling` | Main Processing has failed, timed out, or been skipped unexpectedly | Records failure and alerts support owner |
| `Scope - Finalise` | Main Processing or Error Handling completes | Optional clean-up or final telemetry |

### Retry Policies

| Action Type | Recommended Retry Policy |
| --- | --- |
| Dataverse Get row | Default retry policy |
| Dataverse Update row | Exponential retry, 4 attempts |
| Dataverse Add row for Exception Action | Exponential retry, 4 attempts |
| Teams notification | Default retry policy |
| Outlook email | Default retry policy |

### Error Handling Steps

1. Capture the failed action details using `result('Scope_-_Main_Processing')`.
2. Add an Exception Action record with Action Type `Flow Error` and Action Status `Failed`.
3. Notify the flow owner or support mailbox with the flow run link.
4. Terminate the flow with status `Failed` so monitoring can detect the issue.

**Error notification subject:**

```text
Power Automate failure: NHSP shift exception escalation
```

**Error notification body:**

```text
The escalation flow failed while processing a shift exception.

Exception ID: @{triggerOutputs()?['body/nhsp_exceptionid']}
Flow run: @{workflow()?['run']['name']}
Failure time: @{utcNow()}

Review the flow run history and Dataverse Exception Action records for details.
```

---

## Security Considerations

- Use a dedicated service account or managed connection reference for the flow.
- Grant least-privilege Dataverse permissions: read Shift, read/update Shift Exception, create Exception Action.
- Store trust contact addresses in environment variables or a Dataverse configuration table, not directly in flow actions.
- Keep notifications limited to operational workforce data.
- Use Dataverse auditing alongside Exception Action records for authoritative change tracking.

## Accessibility Considerations

- Keep Teams and email messages short, structured, and readable with clear labels.
- Avoid relying on colour or emoji in notification content.
- Use descriptive subject lines so coordinators can triage from assistive technologies and mobile previews.

## Assumptions

| # | Assumption |
| --- | --- |
| 1 | Shift Exception has a required lookup to Shift. |
| 2 | Shift start times are stored in Dataverse Date and Time format and compared in UTC in the flow. |
| 3 | Trust contact routing is available through environment variables or a small configuration table. |
| 4 | Exception Action is added to the solution as an audit table. |
| 5 | The flow is packaged in the same managed solution as the canvas app and Dataverse tables. |

## Risks

| Risk | Mitigation |
| --- | --- |
| Duplicate notifications when a record is updated repeatedly | Use trigger conditions and check `nhsp_EscalationStatus` before sending notifications. |
| Flow delay continues after an exception is resolved | Re-read the Shift Exception after the delay before escalating. |
| Choice values differ between environments | Use environment-specific numeric choice values or confirm formatted values during deployment. |
| Trust contact is missing or incorrect | Validate contact configuration during deployment and write a failed Exception Action if no route is found. |
| Notification contains too much staff information | Keep message content to shift and exception context only. |

---

## Test Cases

| # | Scenario | Test Data | Expected Result |
| --- | --- | --- | --- |
| 1 | Critical exception created for shift starting in 4 hours | Priority `Critical`, Status `Open`, Start Time `utcNow() + 4 hours` | Coordinator notification sent, Exception Action created, Escalation Status set to Pending Trust Response |
| 2 | High exception created for shift starting in 20 hours | Priority `High`, Status `Open`, Start Time `utcNow() + 20 hours` | Coordinator notification sent and audit record created |
| 3 | Medium exception created for shift starting in 4 hours | Priority `Medium`, Status `Open` | Flow does not notify; terminates successfully or does not trigger depending on trigger condition |
| 4 | Critical exception for shift starting in 30 hours | Priority `Critical`, Start Time `utcNow() + 30 hours` | Flow skips escalation and writes a skipped Exception Action if the main flow runs |
| 5 | Critical exception resolved during delay period | Status changes to `Resolved` before delay ends | Trust escalation is not sent; Exception Action notes that escalation was skipped |
| 6 | Critical exception remains unresolved after delay | Status remains `Open` or `In Progress` | Trust contact notification sent, Exception Action created, Escalation Status set to Escalated |
| 7 | Dataverse update fails temporarily | Simulate transient connector failure | Retry policy attempts the update; failure scope runs only if retries are exhausted |
| 8 | Trust contact configuration missing | Trust has no configured contact route | Flow writes Flow Error Exception Action and alerts support owner |

## Deployment Notes

- Use connection references for Dataverse, Teams, and Outlook connectors.
- Store response windows, support mailbox, and trust contact routing as environment variables.
- Turn on flow run-only user restrictions through solution ownership and environment security roles.
- Test in a non-production Power Platform environment using fictional workforce data before importing to the demo environment.
