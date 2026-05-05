# Deployment Strategy

## Overview

This document describes the application lifecycle management approach for the **NHSP Shift Exception Manager** Power Platform solution. It is written for a realistic development, test, and production setup while remaining lightweight enough for a demo solution.

The solution is packaged as a Dataverse solution named `nhsp_ShiftExceptionManagement` and includes:

- Power Apps canvas app for exception triage.
- Dataverse tables, columns, choices, relationships, security roles, and audit settings.
- Power Automate cloud flows for escalation and notification.
- Environment variables and connection references for environment-specific configuration.
- Sample data and documentation stored in GitHub.

## Environment Strategy

Use separate Power Platform environments to keep development changes away from test validation and production users.

| Environment | Purpose | Solution Type | Data | Access |
| --- | --- | --- | --- | --- |
| Development | Build and configure app, Dataverse, and flows | Unmanaged | Synthetic sample data | Makers, developers, solution owners |
| Test | Validate deployment package, security roles, flows, and demo scripts | Managed | Controlled synthetic test data | Testers, product owner, selected operations users |
| Production | Run the approved demo or live pilot | Managed | Approved demo or production-safe operational data | Operations users and support owners only |

### Development

- Makers work in the unmanaged `nhsp_ShiftExceptionManagement` solution.
- Changes are exported from Development and committed to GitHub for review.
- Synthetic data only. Do not use real NHS staff data or patient clinical data.
- Cloud flows can be disabled by default when testing configuration-only changes.

### Test

- Import the managed solution package from the reviewed branch or release artefact.
- Configure environment variables, connection references, and security role assignments.
- Run functional, security, accessibility, and escalation-flow tests before promotion.
- Use a test trust-contact mailbox or Teams channel, never real escalation recipients.

### Production

- Import only reviewed and approved managed solution versions.
- Use production connection references owned by a service account or managed support account.
- Restrict maker access. Production changes should not be made directly except for emergency operational fixes approved by the solution owner.
- Monitor cloud flow runs and Dataverse audit logs after deployment.

---

## Solution Export And Import Approach

### Development Export

1. Confirm all components are inside the `nhsp_ShiftExceptionManagement` solution.
2. Publish all customisations in Development.
3. Run Solution Checker and address high-impact issues before export.
4. Export an unmanaged solution for source control.
5. Export a managed solution for Test and Production deployment.
6. Unpack the unmanaged solution into source control using Power Platform CLI.

Example commands:

```powershell
pac auth select --name nhsp-dev
pac solution export --name nhsp_ShiftExceptionManagement --path ./out/nhsp_ShiftExceptionManagement_unmanaged.zip --managed false
pac solution export --name nhsp_ShiftExceptionManagement --path ./out/nhsp_ShiftExceptionManagement_managed.zip --managed true
pac solution unpack --zipfile ./out/nhsp_ShiftExceptionManagement_unmanaged.zip --folder ./src/solutions/nhsp_ShiftExceptionManagement --packagetype Unmanaged
```

### Test Import

1. Build or retrieve the managed solution artefact from the approved pull request.
2. Import into Test as a managed solution.
3. Apply deployment settings for environment variables and connection references.
4. Assign Dataverse security roles to test Entra ID groups.
5. Turn on cloud flows after connection references are validated.
6. Run the test cases documented for the app, data model, and flows.

Example command:

```powershell
pac auth select --name nhsp-test
pac solution import --path ./out/nhsp_ShiftExceptionManagement_managed.zip --publish-changes true
```

### Production Import

1. Confirm Test sign-off is recorded in the pull request or release notes.
2. Take a backup or create a restore point for the production Dataverse environment.
3. Import the same managed solution package validated in Test.
4. Apply production deployment settings.
5. Validate app launch, key Dataverse permissions, cloud flow ownership, and notification routing.
6. Monitor early flow runs and audit records.

Production deployments should use the exact managed solution version tested in Test. Avoid rebuilding between Test and Production unless a new test cycle is completed.

---

## Source Control And Pull Request Review

GitHub is the source of record for unpacked solution files, documentation, sample data, and ALM scripts.

### Branching

| Branch | Purpose |
| --- | --- |
| `main` | Approved, deployable baseline |
| `feature/*` | App, Dataverse, flow, or documentation changes |
| `release/*` | Optional stabilisation branch for demo or pilot releases |

### Pull Request Requirements

Every change should be reviewed through a pull request before it is deployed beyond Development.

Review checklist:

- Business context is clear and linked to a user need.
- Dataverse schema changes are intentional and documented.
- Canvas app changes are accessible and support keyboard/screen reader use where relevant.
- Power Automate flows include error handling and do not expose unnecessary staff information.
- Environment variables and connection references are used for environment-specific values.
- Security roles follow least privilege.
- Solution Checker results are reviewed.
- Test cases are updated or confirmed unchanged.
- No patient clinical data or real NHS staff data is committed.

### Review Roles

| Reviewer | Focus |
| --- | --- |
| Solution owner | Business fit and demo readiness |
| Power Platform maker | App, flow, and Dataverse configuration quality |
| Security reviewer | Roles, data exposure, connection ownership, auditability |
| Tester or product owner | User journey and acceptance criteria |

---

## Environment Variables

Environment variables hold values that change between Development, Test, and Production without changing the solution components.

| Variable | Example Development Value | Example Test Value | Example Production Value | Purpose |
| --- | --- | --- | --- | --- |
| `nhsp_SupportMailbox` | `nhsp-demo-support-dev@example.invalid` | `nhsp-demo-support-test@example.invalid` | Production support mailbox | Flow failure notifications |
| `nhsp_DefaultTrustContactEmail` | `trust-contact-dev@example.invalid` | `trust-contact-test@example.invalid` | Approved trust escalation address | Escalation fallback route |
| `nhsp_CriticalEscalationDelayMinutes` | `5` | `30` | `120` | Delay before escalating Critical exceptions |
| `nhsp_HighEscalationDelayMinutes` | `10` | `60` | `240` | Delay before escalating High exceptions |
| `nhsp_AppDeepLinkUrl` | Development app URL | Test app URL | Production app URL | Link included in notifications |
| `nhsp_EnableExternalNotifications` | `false` | `false` | `true` | Controls whether trust contact notifications are sent |

Deployment notes:

- Store non-secret configuration in Power Platform environment variables.
- Store secrets, if required, in secure connector configuration or a suitable secret store rather than plain environment variable values.
- Document each variable's owner and expected format.
- Validate all required variables immediately after import and before turning on flows.

---

## Connection References

Connection references keep connector configuration environment-specific while preserving the same flow definition across environments.

| Connection Reference | Connector | Owner | Used By | Notes |
| --- | --- | --- | --- | --- |
| `cr_NHSP_Dataverse` | Microsoft Dataverse | Service account or deployment owner | Canvas app and flows | Requires least-privilege Dataverse roles |
| `cr_NHSP_Outlook` | Office 365 Outlook | Service account | Escalation flow notifications | Use shared mailbox where appropriate |
| `cr_NHSP_Teams` | Microsoft Teams | Service account | Coordinator notifications | Use test channels outside Production |

Guidance:

- Use service-owned connections for flows that must run independently of individual makers.
- Rebind connection references during import into Test and Production.
- Confirm connector data loss prevention policies allow the required connector combination.
- Avoid personal maker-owned connections in Production.
- Document connection ownership and renewal responsibilities.

---

## Deployment Validation

Run these checks after each Test or Production import.

| Area | Validation |
| --- | --- |
| Solution | Version number, publisher, and managed state are correct |
| Dataverse | Tables, columns, relationships, choices, and audit settings are present |
| Security | Entra ID groups are mapped to the correct Dataverse security roles |
| Canvas app | App opens, filters exceptions, displays details, and saves actions |
| Cloud flows | Flows are turned on, connection references are valid, and test runs succeed |
| Environment variables | All required values are populated for the target environment |
| Notifications | Test messages route to approved test or production recipients |
| Auditability | Dataverse audit and Exception Action records are written as expected |
| Accessibility | Key screens can be navigated by keyboard and labels are readable |

---

## Rollback Considerations

Rollback planning is important because managed solution imports can change schema, flows, and app behaviour.

### Before Deployment

- Record the currently installed managed solution version.
- Export or retain the previous managed solution package.
- Create a Dataverse environment backup or restore point before Production import.
- Capture current environment variable values and connection reference bindings.
- Confirm who can approve rollback during the deployment window.

### Rollback Options

| Situation | Preferred Rollback |
| --- | --- |
| Canvas app issue only | Import previous managed solution version or restore previous app version if available |
| Flow notification issue | Turn off affected flow, correct configuration, then re-enable after testing |
| Environment variable error | Update variable value and rerun validation |
| Connection reference issue | Rebind connection reference and re-enable flows |
| Dataverse schema issue | Restore environment backup if schema change cannot be safely reversed |
| Data corruption or unexpected data change | Stop flows, restore backup, and review audit records before redeployment |

### Rollback Principles

- Do not delete managed solution components manually in Production unless this is part of an approved rollback plan.
- Prefer restoring the last known-good managed solution package for app and flow behaviour issues.
- Treat destructive schema changes as high risk. Avoid deleting columns or tables unless there is a tested migration and rollback path.
- Keep flow run history, Dataverse audit logs, and Exception Action records for investigation.
- Communicate rollback status to operations users, especially if escalation notifications are paused.

---

## Security And Compliance Considerations

- Use synthetic data for Development and Test.
- Do not commit secrets, real staff data, or patient clinical data to GitHub.
- Apply least privilege to Dataverse roles and flow connection owners.
- Use UK-hosted Power Platform environments for realistic NHS data residency alignment.
- Enable auditing on key Dataverse tables and columns.
- Review access to Production quarterly through Entra ID group membership.

## Assumptions

| # | Assumption |
| --- | --- |
| 1 | The solution is packaged as a single Dataverse solution named `nhsp_ShiftExceptionManagement`. |
| 2 | Power Platform CLI is available to makers or the build pipeline. |
| 3 | Development uses unmanaged solutions; Test and Production use managed solutions. |
| 4 | GitHub pull requests are required before promotion to Test or Production. |
| 5 | Environment variables and connection references are configured during import, not hard-coded in flows. |

## Risks

| Risk | Mitigation |
| --- | --- |
| Direct Production edits bypass source control | Restrict maker access and require emergency changes to be backported to Development and GitHub |
| Incorrect environment variable values cause misrouted notifications | Use deployment settings files and post-import validation |
| Personal connections break when a maker leaves | Use service-owned connection references for Production flows |
| Managed solution import introduces unexpected behaviour | Deploy the exact Test-approved package and keep a previous package for rollback |
| Test data resembles real staff data too closely | Use clearly synthetic names, shifts, trusts, and contact addresses |

## Test Cases

| # | Scenario | Expected Result |
| --- | --- | --- |
| 1 | Import managed solution into Test | Import succeeds, components appear in managed state, flows remain off until configured |
| 2 | Configure environment variables | Flow actions read Test values and route notifications to test recipients |
| 3 | Rebind connection references | Dataverse, Outlook, and Teams actions validate successfully |
| 4 | Submit pull request with solution change | Review checklist is completed before merge |
| 5 | Deploy approved package to Production | Same managed solution version from Test is imported and validated |
| 6 | Roll back a faulty flow configuration | Flow is disabled or previous package restored, and users are informed |
