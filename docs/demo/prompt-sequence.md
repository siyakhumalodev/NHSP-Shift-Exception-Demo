# Demo Prompt Sequence

## Purpose

Use this sequence to keep the 35-minute Power Platform segment focused. Each prompt should produce a visible artefact or review output that advances the delivery story.

## Sequence

| Order | Prompt File | Intended Output | Presenter Note |
| --- | --- | --- | --- |
| 1 | `.github/prompts/create-github-issues.prompt.md` | GitHub Issues backlog items | Use during the first 10 minutes with GitHub |
| 2 | `.github/prompts/create-power-platform-design.prompt.md` | Solution overview, Dataverse model, canvas screens, security model | Show that Copilot reads samples and instructions |
| 3 | `.github/prompts/generate-power-fx-formulas.prompt.md` | Practical Power Fx formula examples | Emphasise assumptions, readable logic, and testability |
| 4 | `.github/prompts/generate-flow-design.prompt.md` | Escalation flow design | Emphasise audit records, retries, and safe notification content |
| 5 | `.github/prompts/review-power-platform-solution.prompt.md` | Senior architect-style readiness review | End with risks, questions, and next backlog items |

## Suggested Chat Commands

Use these in Copilot Chat, adjusting phrasing if needed for the environment.

```text
Follow instructions in #prompt:create-github-issues.prompt.md
```

```text
Follow instructions in #prompt:create-power-platform-design.prompt.md
```

```text
Follow instructions in #prompt:generate-power-fx-formulas.prompt.md
```

```text
Follow instructions in #prompt:generate-flow-design.prompt.md
```

```text
Follow instructions in #prompt:review-power-platform-solution.prompt.md
```

## Time Control

| Constraint | Adjustment |
| --- | --- |
| Running behind | Generate only one or two artefacts, then show existing docs for the rest |
| Output is too broad | Ask Copilot to focus on one file or one section |
| Audience is product-heavy | Spend more time on requirements, journey, and GitHub Issues backlog |
| Audience is technical | Spend more time on Dataverse, security, flow reliability, and ALM |

## Review Checklist Before Starting

- README explains the 45-minute split.
- Product requirements and user journey are populated.
- Sample data is synthetic and safe to show.
- Prompt files are visible under `.github/prompts`.
- Existing generated artefacts are either intentionally present or moved aside for a clean generation demo.
