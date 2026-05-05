# Security Model

## Overview

The security model uses Dataverse security roles and Microsoft Entra ID to control access to data and app functionality. The design follows the principle of least privilege — users can only see and modify data relevant to their role.

## Authentication

- **Identity provider:** Microsoft Entra ID (formerly Azure AD)
- **MFA:** Required for all users per NHS Digital security guidance
- **Conditional Access:** Restrict access to managed devices and approved locations (recommended for production; optional for demo)

## Security Roles

Three custom Dataverse security roles are defined within the `nhsp_ShiftExceptionManagement` solution.

### 1. NHSP Operations Coordinator

**Intended for:** Front-line coordinators who triage exceptions day-to-day.

| Table | Create | Read | Update | Delete | Append | Append To |
|---|---|---|---|---|---|---|
| `nhsp_ShiftException` | ✅ Org | ✅ Org | ✅ Org | ❌ | ✅ | ✅ |
| `nhsp_Shift` | ❌ | ✅ Org | ❌ | ❌ | ✅ | ✅ |
| `nhsp_Worker` | ❌ | ✅ Org | ❌ | ❌ | — | — |

**Notes:**
- Can create and update exceptions (change status, add resolution notes, escalate).
- Read-only access to shifts and workers.
- Cannot delete any records.

### 2. NHSP Compliance Officer

**Intended for:** Users who review compliance-related exceptions.

| Table | Create | Read | Update | Delete | Append | Append To |
|---|---|---|---|---|---|---|
| `nhsp_ShiftException` | ❌ | ✅ Org | ✅ Org | ❌ | ✅ | ✅ |
| `nhsp_Shift` | ❌ | ✅ Org | ❌ | ❌ | — | — |
| `nhsp_Worker` | ❌ | ✅ Org | ✅ Org | ❌ | — | — |

**Notes:**
- Can update exception status and resolution notes for compliance blockers.
- Can update worker compliance status (e.g. mark training as renewed).
- Cannot create or delete records.

### 3. NHSP Operations Manager

**Intended for:** Senior managers overseeing escalations and KPIs.

| Table | Create | Read | Update | Delete | Append | Append To |
|---|---|---|---|---|---|---|
| `nhsp_ShiftException` | ✅ Org | ✅ Org | ✅ Org | ✅ Org | ✅ | ✅ |
| `nhsp_Shift` | ✅ Org | ✅ Org | ✅ Org | ❌ | ✅ | ✅ |
| `nhsp_Worker` | ❌ | ✅ Org | ✅ Org | ❌ | — | — |

**Notes:**
- Full control over exceptions including delete (for removing duplicates).
- Can create and update shifts.
- Read and update access to workers.

## Role Assignment

| Entra ID Group | Dataverse Security Role | Purpose |
|---|---|---|
| `SG-NHSP-OpsCoordinators` | NHSP Operations Coordinator | Day-to-day triage users |
| `SG-NHSP-ComplianceOfficers` | NHSP Compliance Officer | Compliance review users |
| `SG-NHSP-OpsManagers` | NHSP Operations Manager | Senior oversight and escalation |

Roles are assigned via Entra ID security groups mapped to Dataverse security roles in the Power Platform admin centre. This avoids per-user role assignment and simplifies onboarding.

## Row-Level Security

For the demo, all roles use **Organisation-level** access (all records visible). In a production deployment, consider:

- **Business Unit scoping** — If trusts map to business units, coordinators could be restricted to seeing only their trust's data.
- **Team ownership** — Exceptions could be owned by teams mapped to trust operations teams.

## Column-Level Security

Not required for the demo. In production, consider column security profiles for:

- `nhsp_Worker.nhsp_FullName` — Restrict to compliance officers and managers only if worker PII needs tighter control.

## App-Level Security

- The canvas app is **shared** with the three Entra ID security groups above.
- Users without a matching security role cannot open the app.
- The app uses `User()` function to display the logged-in user and conditionally show/hide the Escalate button (visible only to Operations Coordinators and Operations Managers).

## Audit and Compliance

| Requirement | Implementation |
|---|---|
| **Change tracking** | Dataverse audit logging enabled on `nhsp_ShiftException` (status changes, escalation changes, owner changes). |
| **Audit log retention** | Default Dataverse retention (30 days); extendable via admin settings. |
| **Data residency** | Power Platform environment provisioned in UK South region to comply with NHS data residency requirements. |
| **Data classification** | All data in this solution is **OFFICIAL** (no patient clinical data). Worker names are treated as staff operational data. |
| **GDPR considerations** | Worker records contain names; a retention policy and deletion process should be defined for production. Out of scope for demo. |
| **Access reviews** | Entra ID access reviews recommended quarterly for the three security groups. |

## Assumptions

| # | Assumption |
|---|---|
| 1 | All demo users have Power Apps per-user or per-app licences. |
| 2 | The demo environment has a single business unit; multi-BU hierarchy is out of scope. |
| 3 | No integration with external identity systems (e.g. NHS Smartcard) for the demo. |
| 4 | Audit logging is enabled at the environment level before the demo. |
