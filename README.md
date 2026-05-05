# NHS Professionals Shift Exception Management Demo

This repository demonstrates how GitHub Copilot can support Power Platform delivery for a realistic NHS Professionals workforce operations scenario.

## Scenario

Operations coordinators need a way to identify, triage, and escalate bank shift exceptions before they create operational risk.

## Demo Flow

This is designed as a 45-minute customer demo.

| Time | Segment | Outcome |
|---|---|---|
| 0-10 min | Product Manager agent with ADO MCP | Create Azure Boards work items for the delivery backlog |
| 10-35 min | GitHub Copilot for Power Platform delivery | Generate or refine solution design, Dataverse model, canvas app screens, security model, Power Fx formulas, and Power Automate flow design |
| 35-45 min | Review and readiness | Review security, accessibility, auditability, ALM, risks, and next backlog items |

Detailed presenter guidance is available in `docs/demo/demo-runbook.md` and `docs/demo/prompt-sequence.md`.

## Repository Story

1. Start with the product context in `docs/product` and the synthetic sample data in `samples`.
2. Use the Product Manager prompt to create Azure Boards backlog items.
3. Use GitHub Copilot to generate Power Platform solution design artefacts.
4. Create or refine Dataverse table design and sample data mapping.
5. Generate Power Fx formulas for the canvas app.
6. Generate Power Automate flow design.
7. Review the solution for security, accessibility, auditability, ALM readiness, and demo risks.

## Technology Areas

- GitHub Copilot
- Azure Boards via ADO MCP
- Power Apps
- Dataverse
- Power Fx
- Power Automate
- Power Platform ALM

## Demo Data

The sample files use fictional workforce operations data only:

- `samples/sample-shifts.csv`
- `samples/sample-workers.csv`
- `samples/sample-exceptions.csv`
- `samples/sample-exception-actions.csv`
- `samples/sample-trust-contacts.csv`

Do not use patient data, clinical context, real NHS staff data, secrets, or real escalation contact details in this repository.