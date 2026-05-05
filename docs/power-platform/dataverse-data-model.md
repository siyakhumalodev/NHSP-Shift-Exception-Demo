# Dataverse Data Model

## Overview

The data model consists of custom Dataverse tables that mirror the operational entities in the sample data. Relationships are enforced via lookup columns to maintain referential integrity.

## Tables

### 1. Shift (`nhsp_Shift`)

Represents a bank shift at a trust ward that requires one or more workers.

| Column | Display Name | Type | Required | Notes |
|---|---|---|---|---|
| `nhsp_ShiftId` | Shift ID | Text (Primary Name) | Yes | Business key, e.g. `SFT-1001` |
| `nhsp_Trust` | Trust | Choice | Yes | e.g. Northshire NHS Trust, Southvale NHS Trust |
| `nhsp_Ward` | Ward | Text | Yes | e.g. Emergency Department, Acute Medical Unit |
| `nhsp_Role` | Role | Choice | Yes | Registered Nurse, Healthcare Assistant |
| `nhsp_StartTime` | Start Time | Date and Time | Yes | Shift start |
| `nhsp_EndTime` | End Time | Date and Time | Yes | Shift end |
| `nhsp_Status` | Status | Choice | Yes | Open, Filled, Cancelled |
| `nhsp_RequiredWorkers` | Required Workers | Whole Number | Yes | Total workers needed |
| `nhsp_FilledWorkers` | Filled Workers | Whole Number | Yes | Workers currently assigned |

**Choice values — Trust:**
- Northshire NHS Trust
- Southvale NHS Trust

**Choice values — Role:**
- Registered Nurse
- Healthcare Assistant

**Choice values — Status:**
- Open
- Filled
- Cancelled

---

### 2. Worker (`nhsp_Worker`)

Represents a bank worker available for shift assignment.

| Column | Display Name | Type | Required | Notes |
|---|---|---|---|---|
| `nhsp_WorkerId` | Worker ID | Text (Primary Name) | Yes | Business key, e.g. `WRK-2001` |
| `nhsp_FullName` | Full Name | Text | Yes | Worker's display name |
| `nhsp_Role` | Role | Choice | Yes | Registered Nurse, Healthcare Assistant |
| `nhsp_ComplianceStatus` | Compliance Status | Choice | Yes | Compliant, Training Expired, DBS Review Required |
| `nhsp_AvailabilityStatus` | Availability | Choice | Yes | Available, Unavailable |
| `nhsp_PreferredTrust` | Preferred Trust | Choice | No | Worker's preferred trust |

**Choice values — Compliance Status:**
- Compliant
- Training Expired
- DBS Review Required

**Choice values — Availability:**
- Available
- Unavailable

---

### 3. Shift Exception (`nhsp_ShiftException`)

Represents an exception raised against a shift that requires triage and resolution.

| Column | Display Name | Type | Required | Notes |
|---|---|---|---|---|
| `nhsp_ExceptionId` | Exception ID | Text (Primary Name) | Yes | Business key, e.g. `EXC-3001` |
| `nhsp_Shift` | Shift | Lookup → `nhsp_Shift` | Yes | The shift this exception relates to |
| `nhsp_ExceptionType` | Exception Type | Choice | Yes | Unfilled Shift, Compliance Blocker, Urgent Staffing Request |
| `nhsp_Priority` | Priority | Choice | Yes | Critical, High, Medium, Low |
| `nhsp_Status` | Status | Choice | Yes | Open, In Progress, Resolved, Closed |
| `nhsp_CreatedTime` | Created Time | Date and Time | Yes | When the exception was raised |
| `nhsp_Owner` | Owner | Lookup → SystemUser | Yes | The user or team responsible |
| `nhsp_EscalationStatus` | Escalation Status | Choice | Yes | Not Escalated, Pending Trust Response, Escalated |
| `nhsp_ResolutionNotes` | Resolution Notes | Multiline Text | No | Free-text notes on how the exception was resolved |
| `nhsp_ResolvedTime` | Resolved Time | Date and Time | No | When the exception was resolved |

**Choice values — Exception Type:**
- Unfilled Shift
- Compliance Blocker
- Urgent Staffing Request

**Choice values — Priority:**
- Critical
- High
- Medium
- Low

**Choice values — Status:**
- Open
- In Progress
- Resolved
- Closed

**Choice values — Escalation Status:**
- Not Escalated
- Pending Trust Response
- Escalated

---

### 4. Exception Action (`nhsp_ExceptionAction`)

Represents an auditable action taken by a coordinator, manager, or Power Automate flow.

| Column | Display Name | Type | Required | Notes |
|---|---|---|---|---|
| `nhsp_ExceptionActionId` | Exception Action ID | Text (Primary Name) | Yes | Business key, e.g. `ACT-4001` |
| `nhsp_ShiftException` | Shift Exception | Lookup -> `nhsp_ShiftException` | Yes | Exception this action relates to |
| `nhsp_ActionType` | Action Type | Choice | Yes | Coordinator Notification, Trust Escalation, Flow Error, Manual Update |
| `nhsp_ActionStatus` | Action Status | Choice | Yes | Sent, Skipped, Failed, Retried, Completed |
| `nhsp_ActionTime` | Action Time | Date and Time | Yes | When the action occurred |
| `nhsp_ActionBy` | Action By | Text | Yes | User display name or flow service account |
| `nhsp_Notes` | Notes | Multiline Text | No | Operational summary without patient or clinical data |

---

### 5. Trust Contact (`nhsp_TrustContact`)

Represents synthetic or configured routing for trust escalation notifications.

| Column | Display Name | Type | Required | Notes |
|---|---|---|---|---|
| `nhsp_TrustContactId` | Trust Contact ID | Text (Primary Name) | Yes | Business key, e.g. `TC-5001` |
| `nhsp_Trust` | Trust | Choice | Yes | Trust this contact route supports |
| `nhsp_ContactName` | Contact Name | Text | Yes | Team or mailbox display name, not a real person in demo data |
| `nhsp_ContactRole` | Contact Role | Text | Yes | Workforce or staffing contact role |
| `nhsp_ContactEmail` | Contact Email | Email | Yes | Synthetic demo address or approved environment value |
| `nhsp_EscalationChannel` | Escalation Channel | Choice | Yes | Email, Teams, Manual |
| `nhsp_IsDemoContact` | Is Demo Contact | Yes/No | Yes | Flags fictional contacts used for demo safety |

---

## Relationships

```
┌──────────────┐        ┌──────────────────────┐
│  nhsp_Shift  │───1:N──│  nhsp_ShiftException │
│              │        │                      │
└──────────────┘        └──────────────────────┘
                                  │
                                  │ 1:N
                                  ▼
                     ┌────────────────────────┐
                     │ nhsp_ExceptionAction   │
                     └────────────────────────┘

┌──────────────┐
│  nhsp_Worker │  (No direct FK to Exception
│              │   in MVP — exceptions are
└──────────────┘   shift-centric, not worker-
                   centric. Workers are used
                   for availability lookups.)
```

| Relationship | Type | Parent | Child | Behaviour |
|---|---|---|---|---|
| Shift → Shift Exception | 1:N | `nhsp_Shift` | `nhsp_ShiftException` | Referential (restrict delete if exceptions exist) |
| Shift Exception -> Exception Action | 1:N | `nhsp_ShiftException` | `nhsp_ExceptionAction` | Referential (restrict delete if audit actions exist) |

## Audit Configuration

- Enable **table-level auditing** on all three tables.
- Enable **column-level auditing** on `nhsp_ShiftException.nhsp_Status`, `nhsp_ShiftException.nhsp_EscalationStatus`, and `nhsp_ShiftException.nhsp_Priority` to track triage decisions.
- Dataverse audit logs are retained per the environment's audit retention policy.

## Sample Data Mapping

The sample CSV files map directly to these tables:

| CSV File | Dataverse Table |
|---|---|
| `sample-shifts.csv` | `nhsp_Shift` |
| `sample-workers.csv` | `nhsp_Worker` |
| `sample-exceptions.csv` | `nhsp_ShiftException` |
| `sample-exception-actions.csv` | `nhsp_ExceptionAction` |
| `sample-trust-contacts.csv` | `nhsp_TrustContact` |
