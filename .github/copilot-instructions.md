# GitHub Copilot Instructions

You are assisting with a Power Platform demo for NHS Professionals.

## Scenario

The solution is a Bank Shift Exception Management App for workforce operations teams.

The app helps operations coordinators identify, triage, and escalate bank shift exceptions such as:
- Unfilled shifts close to start time
- Worker compliance blockers
- Urgent staffing requests
- Escalation of unresolved shift risks

## Delivery Style

When generating content:
- Keep the solution realistic for a healthcare workforce organisation.
- Avoid patient clinical data.
- Focus on operational workforce data.
- Use clear, simple Power Platform terminology.
- Explain Power Platform concepts where needed.
- Include security, accessibility, and auditability considerations.

## Preferred Architecture

- Power Apps canvas app for the user interface
- Dataverse for structured data
- Power Automate for escalation workflows
- GitHub for source control, documentation, review, and ALM artefacts
- Azure Boards for work item planning

## Output Expectations

When creating artefacts, include:
- Business context
- User need
- Technical approach
- Security considerations
- Accessibility considerations
- Test cases
- Assumptions
- Risks

## Do Not

- Use real patient data.
- Use real NHS staff data.
- Assume clinical decision-making.
- Overcomplicate the solution with unnecessary Azure services.