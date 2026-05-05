---
description: "Use when: creating GitHub Issues for the NHS Professionals Shift Exception Management demo backlog."
---

# Create GitHub Issues

Using the NHS Professionals Bank Shift Exception Management scenario, create GitHub Issues for the Power Platform delivery backlog.

Use the repository context from:

- `README.md`
- `.github/copilot-instructions.md`
- `docs/product/shift-exception-requirements.md`
- `docs/product/shift-exception-user-journey.md`
- `samples/sample-shifts.csv`
- `samples/sample-workers.csv`
- `samples/sample-exceptions.csv`

Create issues that cover:

1. Exception dashboard and filtering
2. Dataverse table design for shifts, workers, exceptions, actions, and trust contacts
3. Canvas app screen design
4. Power Fx formulas for filtering, risk scoring, validation, empty states, and save errors
5. Power Automate escalation flow
6. Security roles and Entra ID group mapping
7. Accessibility validation
8. ALM and deployment readiness
9. Demo data preparation
10. Solution review before customer demonstration

For each issue, include:

- Title
- Suggested labels, such as `power-platform`, `dataverse`, `power-apps`, `power-automate`, `security`, `accessibility`, `alm`, or `demo`
- Business context
- User need
- Acceptance criteria
- Security considerations
- Accessibility considerations
- Test cases
- Assumptions
- Risks

Keep the backlog practical for a 45-minute demo. Do not include patient clinical data, real NHS staff data, or clinical decision-making requirements.
