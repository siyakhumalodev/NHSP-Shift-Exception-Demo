# Environment Strategy

## Business Context

This strategy supports a realistic Power Platform delivery path for the NHSP Shift Exception Manager demo. It keeps the demo lightweight while showing the controls a healthcare workforce organisation would expect around data, access, promotion, and auditability.

## Environment Overview

| Environment | Purpose | Data | Access | Notes |
| --- | --- | --- | --- | --- |
| Development | Build and iterate on Dataverse, app, flow, and documentation | Synthetic sample data | Makers and delivery team | Uses unmanaged solution components |
| Test | Validate deployment, roles, accessibility, flows, and demo scripts | Controlled synthetic test data | Product owner, testers, selected reviewers | Uses managed solution import |
| Demo or Production Pilot | Present approved solution or run a controlled pilot | Approved demo data or production-safe operational data | Named operations users and support owners | Uses managed solution import and service-owned connections |

## Development Environment

| Topic | Approach |
| --- | --- |
| Solution type | Unmanaged solution named `nhsp_ShiftExceptionManagement` |
| Data | Fictional trusts, wards, workers, shifts, exceptions, actions, and contacts |
| Access | Makers and solution owners only |
| Flow state | Flows can remain off unless actively tested |
| Source control | Export and unpack changes into GitHub for review |

## Test Environment

| Topic | Approach |
| --- | --- |
| Solution type | Managed solution imported from reviewed package |
| Data | Synthetic test records aligned with sample CSV scenarios |
| Access | Testers, product owner, security reviewer, and demo owner |
| Configuration | Environment variables and connection references configured after import |
| Validation | Run functional, security, accessibility, escalation, and ALM test cases |

## Demo Or Production Pilot Environment

| Topic | Approach |
| --- | --- |
| Solution type | Managed solution version already validated in Test |
| Data | Demo-safe data only unless a formal production pilot has been approved |
| Access | Entra ID groups mapped to Dataverse roles |
| Connections | Service-owned Dataverse, Outlook, and Teams connections |
| Monitoring | Cloud flow run history, Dataverse audit logs, and Exception Action records reviewed after deployment |

## Environment Variables

| Variable | Purpose | Development | Test | Demo Or Production Pilot |
| --- | --- | --- | --- | --- |
| `nhsp_SupportMailbox` | Flow failure notifications | Demo support mailbox | Test support mailbox | Approved support mailbox |
| `nhsp_DefaultTrustContactEmail` | Fallback escalation recipient | Synthetic address | Test mailbox | Approved contact route |
| `nhsp_CriticalEscalationDelayMinutes` | Critical delay window | 5 | 30 | 120 or agreed operational value |
| `nhsp_HighEscalationDelayMinutes` | High delay window | 10 | 60 | 240 or agreed operational value |
| `nhsp_EnableExternalNotifications` | Prevent accidental external messages | false | false | true only after approval |

## Security Considerations

- Use Microsoft Entra ID groups for app sharing and Dataverse role assignment.
- Use least-privilege roles for operations coordinators, compliance officers, and managers.
- Use service-owned connection references in Test and Demo or Production Pilot environments.
- Do not place secrets, real staff data, or patient clinical data in environment variables, CSV files, prompts, or documentation.
- Configure DLP policies so Dataverse, Outlook, and Teams connectors are allowed for the solution and unsuitable connectors are blocked.

## Accessibility Considerations

- Validate key screens with keyboard-only navigation in Test before the demo.
- Check priority and status colours against WCAG 2.1 AA contrast expectations.
- Confirm all action controls, filters, and gallery rows have meaningful accessible labels.
- Validate Teams and email notification content for clear subject lines and readable structure.

## Test Cases

| # | Scenario | Expected Result |
| --- | --- | --- |
| 1 | Import managed solution into Test | Components import successfully and flows remain off until configured |
| 2 | Configure environment variables | Flows use test addresses and short demo-safe delay values |
| 3 | Rebind connection references | Dataverse, Outlook, and Teams actions validate without personal maker connections |
| 4 | Assign Dataverse roles through Entra groups | Users receive only the permissions required for their role |
| 5 | Run escalation test with synthetic data | Coordinator notification and Exception Action audit records are created |

## Assumptions And Risks

| Type | Detail | Mitigation |
| --- | --- | --- |
| Assumption | The live demo can use Development or Test depending on tenant availability | Keep the runbook clear about which environment is being shown |
| Assumption | No live external trust escalation recipients are used during the demo | Keep `nhsp_EnableExternalNotifications` false unless explicitly approved |
| Risk | Personal connections expire or fail during the demo | Use service-owned connections and pre-test flow runs |
| Risk | Environment variables are incomplete after import | Run deployment validation before turning on flows |
| Risk | Audience asks about production governance | Refer to deployment strategy, security model, and this environment strategy |
