# Demo Assumptions

## Business Context

This repository supports a demonstration of GitHub Copilot and Power Platform delivery practices for an NHS Professionals workforce operations scenario. It is not a production deployment package and does not represent an approved NHS Professionals operating model.

## Assumptions

| # | Assumption |
| --- | --- |
| 1 | All trusts, wards, workers, shifts, exceptions, contacts, and action records are fictional. |
| 2 | The scenario is limited to operational workforce coordination. It does not include patient data or clinical decisions. |
| 3 | Azure Boards work items are created in the first 10 minutes using ADO MCP. |
| 4 | GitHub Copilot is used during the remaining 35 minutes to create or refine Power Platform delivery artefacts. |
| 5 | The target architecture is Power Apps canvas app, Dataverse, Power Automate, GitHub, and Azure Boards. |
| 6 | The demo focuses on delivery acceleration, governance thinking, and quality review, not a complete built app. |

## Security Boundary

- Do not enter real NHS staff data, patient data, credentials, secrets, or internal contact details into prompts or sample files.
- Use synthetic email domains such as `example.invalid` for escalation examples.
- Keep compliance information at operational status level, such as `Training Expired` or `DBS Review Required`.
- Treat staff names as personal data in production discussions, even though demo names are fictional.

## Accessibility Boundary

- The artefacts describe intended accessible behaviour, including labels, contrast, keyboard navigation, and readable notifications.
- A production implementation would still require testing in Power Apps Studio and with assistive technology where appropriate.

## Auditability Boundary

- Dataverse audit logs and Exception Action records are included in the design to show traceability.
- The sample Exception Action data is illustrative and should not be treated as live evidence.

## Risks To State Clearly

| Risk | Demo Position |
| --- | --- |
| Audience asks whether this is live NHS Professionals data | Confirm that all data is synthetic and demo-only |
| Audience asks whether staffing decisions are clinical | Confirm that the workflow supports operational escalation only |
| Audience asks about production go-live | Refer to security, ALM, environment, and deployment docs as next-step artefacts |
| Audience asks about integration with rostering systems | Position integration as a future backlog item, not part of the demo scope |
