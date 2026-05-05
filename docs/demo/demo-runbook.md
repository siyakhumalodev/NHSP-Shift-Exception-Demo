# 45-Minute Demo Runbook

## Purpose

This runbook supports a 45-minute NHS Professionals demo showing how agents and GitHub Copilot can accelerate Power Platform delivery for a Bank Shift Exception Management App.

## Demo Shape

| Time | Segment | Presenter Goal | Repository Evidence |
| --- | --- | --- | --- |
| 0-2 min | Set context | Explain operational workforce exception scenario and synthetic data boundary | `README.md`, `.github/copilot-instructions.md` |
| 2-10 min | Product Manager agent with ADO MCP | Create Azure Boards work items for the app backlog | `.github/prompts/create-ado-work-items.prompt.md` |
| 10-15 min | Repository context | Show README, instructions, product requirements, journey, and samples | `docs/product/*`, `samples/*` |
| 15-23 min | Generate core Power Platform design | Use Copilot prompt to create or refine solution, Dataverse, canvas app, and security artefacts | `.github/prompts/create-power-platform-design.prompt.md` |
| 23-29 min | Generate Power Fx formulas | Show practical app logic for filters, risk score, validation, empty states, and save errors | `.github/prompts/generate-power-fx-formulas.prompt.md` |
| 29-35 min | Generate flow design | Show escalation automation, audit records, notifications, and retries | `.github/prompts/generate-flow-design.prompt.md` |
| 35-41 min | Review readiness | Ask Copilot for architecture, security, accessibility, ALM, and testing review | `.github/prompts/review-power-platform-solution.prompt.md` |
| 41-45 min | Close | Summarise value, assumptions, and next steps | `docs/demo/demo-assumptions.md`, `docs/demo/prompt-sequence.md` |

## Presenter Setup

1. Confirm the VS Code workspace is opened at the repository root.
2. Confirm the ADO MCP connection is available for the first 10-minute segment.
3. Confirm GitHub Copilot Chat can see repository instructions.
4. Keep sample CSVs open in tabs for quick context switching.
5. Decide whether Copilot will create new artefacts or refine existing artefacts.
6. If demonstrating generation from a clean baseline, move generated docs to a backup branch or start from a clean demo branch.

## Talk Track

| Moment | Key Message |
| --- | --- |
| Opening | This is an operational workforce demo, not a clinical decision-making system. |
| Product Manager agent | Agentic planning turns a scenario into trackable Azure Boards work items. |
| Copilot context | Copilot becomes more useful when the repo contains instructions, samples, and prompts. |
| Power Platform design | Copilot can draft practical Dataverse, canvas app, security, and ALM artefacts. |
| Power Fx | Copilot can turn user needs into readable app formulas with assumptions and tests. |
| Power Automate | Copilot can design reliable escalation flows with retries, audit records, and safe notification content. |
| Review | Copilot can critique the solution for risks before a customer conversation. |

## Fallbacks

| Risk | Fallback |
| --- | --- |
| ADO MCP is unavailable | Show the work item prompt and explain the intended backlog output |
| Copilot output is too long | Ask for the top section only or generate one artefact at a time |
| Flow delays are impractical | Use the design doc and sample Exception Action records |
| Live generation changes too much | Use Git diff to show focused changes and explain review before merge |
| Audience asks about production readiness | Move to security, ALM, and environment strategy docs |

## Success Criteria

- The audience understands how the first 10 minutes create product backlog context.
- The audience sees Copilot use repository instructions and sample data to produce Power Platform artefacts.
- The demo avoids patient data, real staff data, and clinical decision-making claims.
- The final review identifies realistic security, accessibility, auditability, and ALM considerations.
