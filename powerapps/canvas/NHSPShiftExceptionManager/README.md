# NHSP Shift Exception Manager Canvas App

This folder contains a source-controlled canvas app implementation for the NHS Professionals shift exception demo.

## Business context

The app supports workforce operations coordinators who need to triage synthetic bank shift exceptions, review shift fill risk, and find compliant available workers during a demo. It uses operational workforce data only.

## User need

Coordinators need one focused interface to:

- Filter open and in-progress shift exceptions.
- Review related shift context without switching systems.
- Escalate or resolve an exception with operational notes.
- Review upcoming shift fill gaps.
- Find available compliant workers by role and preferred trust.

## Technical approach

The app source is represented as Power Apps `.pa.yaml` files under `Src/`:

- `App.pa.yaml` defines app startup variables, named formulas, Dataverse table data sources, and screen order.
- `scrExceptionDashboard.pa.yaml` implements the dashboard, filters, exception gallery, detail panel, escalation, and resolution actions.
- `scrShiftOverview.pa.yaml` implements the shift fill overview.
- `scrWorkerAvailability.pa.yaml` implements worker filtering and compliance/availability badges.
- `scrExceptionDetail.pa.yaml` implements full exception edit and save behaviour.

The source expects these Dataverse tables to exist:

- `nhsp_shift`
- `nhsp_worker`
- `nhsp_shiftexception`
- `nhsp_exceptionaction`
- `nhsp_trustcontact`

Run the schema and synthetic data helper first:

```powershell
.\tools\deployment\build-dataverse-schema.ps1 -ImportSampleData
```

Validate the canvas source:

```powershell
.\powerapps\canvas\NHSPShiftExceptionManager\build-canvas-app.ps1
```

This validates the checked-in `.pa.yaml` source files. The installed PAC CLI cannot generate a blank Dataverse-backed `.msapp`, and `pac canvas pack` requires a Studio-exported `CanvasManifest.json` layout before packaging.

## Security considerations

- The sample app uses synthetic workforce-operational records only.
- Do not enter patient data, clinical notes, real NHS staff personal data, or real escalation contacts.
- Dataverse security roles must enforce who can read and update shift exceptions.
- Resolution notes should stay operational and audit-friendly.

## Accessibility considerations

- Main galleries, filters, and action buttons include accessible labels.
- Status and priority use text as well as colour.
- Empty state text uses polite live-region behaviour.
- The layout follows a predictable left navigation, filter, gallery, detail sequence.

## Test cases

| Test | Expected result |
| --- | --- |
| Open the app | Dashboard opens first with open/in-progress exceptions. |
| Filter by `High` priority | Only high priority exceptions remain visible. |
| Select `EXC-3001` | Detail panel shows related `SFT-1001` shift context. |
| Escalate an exception | Escalation status updates to `Escalated` or shows a friendly error. |
| Resolve an exception | Status updates to `Resolved`, resolved time is set, and notes are saved. |
| Shift overview for 06 May 2026 | Synthetic shifts show fill ratios and near-start risk styling. |
| Worker availability filtered to compliant workers | Only compliant synthetic workers remain visible. |

## Assumptions

- The app is a tablet canvas app using a 16:9 layout.
- Dataverse choice labels match the schema builder values.
- Lookup column names match the deployed schema, including `nhsp_shift` and reference columns such as `nhsp_exceptionreference`.
- The Power Apps maker will connect the generated source to the target Dataverse environment during import/opening.

## Risks

- The local build script checks source YAML structure, not every runtime formula or Dataverse connector binding.
- Some Dataverse choice formulas may require final adjustment in Power Apps Studio depending on how the maker portal binds choice columns in the target tenant.
- PAC CLI cannot generate a blank Dataverse-backed `.msapp` directly; packaging may require a Studio-created app/template before final ALM export.
