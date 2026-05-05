# Power Fx Formulas

## Overview

This document provides practical Power Fx formulas for the **NHSP Shift Exception Manager** canvas app. The formulas support the main exception triage workflow for operations coordinators using Dataverse-backed operational workforce data.

The examples assume a canvas app connected to these Dataverse tables:

| Display Name | Logical Table Name | Purpose |
| --- | --- | --- |
| Shift | `nhsp_Shift` | Bank shift requiring worker cover |
| Worker | `nhsp_Worker` | Bank worker availability and compliance |
| Shift Exception | `nhsp_ShiftException` | Exception raised against a shift |

## Common Assumptions

The formulas use these app control names. Rename them if the implementation uses different naming conventions.

| Control | Type | Purpose |
| --- | --- | --- |
| `galExceptions` | Gallery | Displays shift exceptions |
| `galWorkers` | Gallery | Displays workers available for assignment |
| `drpTrust` | Dropdown or Combo box | Trust filter |
| `drpWard` | Dropdown or Combo box | Ward filter |
| `drpPriority` | Dropdown or Combo box | Priority filter |
| `drpStatus` | Dropdown or Combo box | Exception status filter |
| `drpExceptionType` | Dropdown or Combo box | Exception type filter |
| `drpWorker` | Dropdown or Combo box | Worker selected for assignment |
| `frmExceptionAction` | Edit form | Captures the exception action or resolution notes |
| `txtActionNotes` | Text input | Notes entered by the coordinator |
| `lblEmptyState` | Label | Friendly message when filters return no records |

For simple dropdown controls, the selected text is referenced with `Selected.Value`. For Dataverse choice columns in canvas apps, the record value may need to be compared with `.Value`, depending on how the data source is configured. For example, use `nhsp_Priority.Value = drpPriority.Selected.Value` if the formula bar expects the choice text.

---

## 1. Filter Open Exceptions By Trust, Ward, Priority, Status, And Exception Type

**Where used:** `galExceptions.Items` on the Exception Dashboard screen.

**What it does:** Shows open or in-progress exceptions and lets the coordinator narrow the list by Trust, Ward, Priority, Status, and Exception Type. Each filter supports an `All` option so the user can clear that filter without resetting the whole dashboard.

```powerfx
SortByColumns(
    Filter(
        nhsp_ShiftExceptions,
        nhsp_Status in ["Open", "In Progress"],
        drpTrust.Selected.Value = "All" Or nhsp_Shift.nhsp_Trust = drpTrust.Selected.Value,
        drpWard.Selected.Value = "All" Or nhsp_Shift.nhsp_Ward = drpWard.Selected.Value,
        drpPriority.Selected.Value = "All" Or nhsp_Priority = drpPriority.Selected.Value,
        drpStatus.Selected.Value = "All" Or nhsp_Status = drpStatus.Selected.Value,
        drpExceptionType.Selected.Value = "All" Or nhsp_ExceptionType = drpExceptionType.Selected.Value
    ),
    "nhsp_CreatedTime",
    SortOrder.Ascending
)
```

**Assumptions:**

- `nhsp_ShiftExceptions` is the canvas app data source for `nhsp_ShiftException`.
- `nhsp_Shift` is a Dataverse lookup column from Shift Exception to Shift.
- `drpTrust`, `drpWard`, `drpPriority`, `drpStatus`, and `drpExceptionType` include an `All` option.
- The dashboard intentionally excludes `Resolved` and `Closed` records from the default working list.

**Security note:** The filter only shapes what the signed-in user sees in the gallery. Dataverse security roles must still restrict table access appropriately for operations coordinators, team leads, and administrators.

---

## 2. Show Exceptions Starting In The Next 24 Hours

**Where used:** `galExceptions.Items` for an urgent view, or a separate `galUrgentExceptions.Items` gallery on the dashboard.

**What it does:** Displays unresolved exceptions where the related shift starts between now and the next 24 hours. This helps coordinators focus on near-term staffing risk.

```powerfx
SortByColumns(
    Filter(
        nhsp_ShiftExceptions,
        nhsp_Status in ["Open", "In Progress"],
        nhsp_Shift.nhsp_StartTime >= Now(),
        nhsp_Shift.nhsp_StartTime <= DateAdd(Now(), 24, TimeUnit.Hours)
    ),
    "nhsp_Shift.nhsp_StartTime",
    SortOrder.Ascending
)
```

**Assumptions:**

- `nhsp_StartTime` is stored as a Dataverse Date and Time column on `nhsp_Shift`.
- The app uses the user's local time zone as presented by `Now()`.
- This formula is for operational prioritisation only and does not make clinical decisions.

**Accessibility note:** If this view is presented as a tab or button, set a clear `AccessibleLabel`, such as `"Show exceptions for shifts starting in the next 24 hours"`.

---

## 3. Calculate A Risk Score

**Where used:** A calculated label in `galExceptions`, a risk score field in the detail panel, or a local collection created in `Screen.OnVisible`.

**What it does:** Produces a simple operational risk score from priority, exception type, shift start time, staffing gap, and escalation status. Higher scores should appear first in triage views.

```powerfx
With(
    {
        hoursToStart: DateDiff(Now(), ThisItem.nhsp_Shift.nhsp_StartTime, TimeUnit.Hours),
        staffingGap: Max(
            ThisItem.nhsp_Shift.nhsp_RequiredWorkers - ThisItem.nhsp_Shift.nhsp_FilledWorkers,
            0
        )
    },
    Switch(
        ThisItem.nhsp_Priority,
        "Critical", 40,
        "High", 30,
        "Medium", 20,
        "Low", 10,
        0
    )
    + Switch(
        ThisItem.nhsp_ExceptionType,
        "Urgent Staffing Request", 25,
        "Compliance Blocker", 20,
        "Unfilled Shift", 15,
        0
    )
    + If(hoursToStart <= 4, 25, If(hoursToStart <= 12, 15, If(hoursToStart <= 24, 10, 0)))
    + If(staffingGap >= 2, 15, If(staffingGap = 1, 8, 0))
    + If(ThisItem.nhsp_EscalationStatus = "Escalated", 10, 0)
)
```

**Assumptions:**

- Used inside a gallery row, so the current exception is available as `ThisItem`.
- `nhsp_RequiredWorkers` and `nhsp_FilledWorkers` are whole number columns on the related Shift.
- Risk scoring is transparent and rules-based for demo purposes. In production, agree the scoring thresholds with workforce operations stakeholders and document governance for any changes.

**Test cases:**

| Scenario | Expected Result |
| --- | --- |
| Critical urgent staffing request starting within 4 hours with two vacancies | Highest score |
| Medium unfilled shift starting tomorrow with one vacancy | Moderate score |
| Low exception on a fully staffed shift more than 24 hours away | Low score |

---

## 4. Display Conditional Priority Labels

**Where used:** Priority label controls inside `galExceptions`, for example `lblPriority.Text`, `lblPriority.Fill`, and `lblPriority.Color`.

**What it does:** Displays a readable priority badge with accessible contrast. The label text is explicit so colour is not the only signal.

### `lblPriority.Text`

```powerfx
Switch(
    ThisItem.nhsp_Priority,
    "Critical", "Critical priority",
    "High", "High priority",
    "Medium", "Medium priority",
    "Low", "Low priority",
    "Priority not set"
)
```

### `lblPriority.Fill`

```powerfx
Switch(
    ThisItem.nhsp_Priority,
    "Critical", ColorValue("#B42318"),
    "High", ColorValue("#B54708"),
    "Medium", ColorValue("#B98900"),
    "Low", ColorValue("#027A48"),
    ColorValue("#667085")
)
```

### `lblPriority.Color`

```powerfx
Color.White
```

### `lblPriority.AccessibleLabel`

```powerfx
"Exception " & ThisItem.nhsp_ExceptionId & ", " & ThisItem.nhsp_Priority & " priority"
```

**Assumptions:**

- The priority label is used inside the exception gallery.
- The app uses text labels alongside colour for accessibility.
- The chosen colours must be checked in the implemented app theme to confirm WCAG 2.1 AA contrast.

---

## 5. Validate Whether A Worker Can Be Assigned To A Shift

**Where used:** `btnAssignWorker.DisplayMode`, `btnAssignWorker.OnSelect`, or validation text near the worker selector on the Worker Availability or Exception Detail screen.

**What it does:** Checks that the selected worker is available, compliant, matched to the shift role, and associated with the same preferred Trust before allowing assignment.

### `btnAssignWorker.DisplayMode`

```powerfx
With(
    {
        selectedWorker: drpWorker.Selected,
        selectedShift: galExceptions.Selected.nhsp_Shift
    },
    If(
        IsBlank(selectedWorker)
            Or selectedWorker.nhsp_AvailabilityStatus <> "Available"
            Or selectedWorker.nhsp_ComplianceStatus <> "Compliant"
            Or selectedWorker.nhsp_Role <> selectedShift.nhsp_Role
            Or selectedWorker.nhsp_PreferredTrust <> selectedShift.nhsp_Trust,
        DisplayMode.Disabled,
        DisplayMode.Edit
    )
)
```

### `lblWorkerValidation.Text`

```powerfx
With(
    {
        selectedWorker: drpWorker.Selected,
        selectedShift: galExceptions.Selected.nhsp_Shift
    },
    If(
        IsBlank(selectedWorker),
        "Select a worker to check assignment readiness.",
        selectedWorker.nhsp_AvailabilityStatus <> "Available",
        "Worker is not currently available.",
        selectedWorker.nhsp_ComplianceStatus <> "Compliant",
        "Worker cannot be assigned until compliance is complete.",
        selectedWorker.nhsp_Role <> selectedShift.nhsp_Role,
        "Worker role does not match the shift role.",
        selectedWorker.nhsp_PreferredTrust <> selectedShift.nhsp_Trust,
        "Worker preferred Trust does not match this shift.",
        "Worker can be assigned to this shift."
    )
)
```

**Assumptions:**

- `drpWorker` is populated from `nhsp_Workers`.
- `galExceptions.Selected.nhsp_Shift` provides the shift being reviewed.
- The MVP does not create a direct worker-to-shift assignment record. This validation supports demo triage and can be connected to an assignment process later.

**Security note:** Compliance blockers should be displayed as operational statuses only. Do not expose unnecessary personal or sensitive staff details in the app.

---

## 6. Show A Friendly Empty State When No Exceptions Match Filters

**Where used:** `lblEmptyState.Visible` and `lblEmptyState.Text` on the Exception Dashboard screen.

**What it does:** Shows a clear message when filters return no exceptions, instead of leaving the gallery area blank.

### `lblEmptyState.Visible`

```powerfx
IsEmpty(galExceptions.AllItems)
```

### `lblEmptyState.Text`

```powerfx
If(
    drpTrust.Selected.Value = "All"
        And drpWard.Selected.Value = "All"
        And drpPriority.Selected.Value = "All"
        And drpStatus.Selected.Value = "All"
        And drpExceptionType.Selected.Value = "All",
    "There are no open shift exceptions to review.",
    "No shift exceptions match the selected filters. Adjust the filters to widen the list."
)
```

### `galExceptions.Visible`

```powerfx
!lblEmptyState.Visible
```

**Assumptions:**

- `galExceptions.Items` contains the filtering formula from section 1.
- The empty state label is placed in the same content area as the gallery.
- The wording avoids implying that staffing risk has disappeared; it only describes the current filtered view.

**Accessibility note:** Set `lblEmptyState.Live` to `Polite` so screen reader users are informed when filter changes remove all results.

---

## 7. Handle Errors When Saving An Exception Action

**Where used:** `btnSaveAction.OnSelect` on the Exception Detail screen, and `frmExceptionAction.OnSuccess` / `frmExceptionAction.OnFailure`.

**What it does:** Saves a status, escalation, or resolution note update and shows a clear success or error notification. Errors are captured without exposing technical Dataverse details to the coordinator.

### `btnSaveAction.OnSelect`

```powerfx
IfError(
    Patch(
        nhsp_ShiftExceptions,
        galExceptions.Selected,
        {
            nhsp_Status: drpStatus.Selected.Value,
            nhsp_ResolutionNotes: txtActionNotes.Text,
            nhsp_ResolvedTime: If(
                drpStatus.Selected.Value in ["Resolved", "Closed"],
                Now(),
                Blank()
            )
        }
    ),
    Notify(
        "The exception action could not be saved. Check your connection and try again.",
        NotificationType.Error
    ),
    Notify(
        "Exception action saved.",
        NotificationType.Success
    );
    Refresh(nhsp_ShiftExceptions)
)
```

### Optional audit-friendly action log collection

Use this local collection during the demo if you want to show what the app attempted to save before reviewing Dataverse audit history.

```powerfx
Collect(
    colExceptionActionLog,
    {
        ExceptionId: galExceptions.Selected.nhsp_ExceptionId,
        ActionStatus: drpStatus.Selected.Value,
        ActionNotesEntered: !IsBlank(txtActionNotes.Text),
        ActionTime: Now(),
        ActionUser: User().Email
    }
)
```

**Assumptions:**

- `galExceptions.Selected` is the exception being updated.
- `drpStatus` contains the target status.
- `txtActionNotes` contains coordinator notes, avoiding patient or clinical data.
- Dataverse auditing is enabled on status, escalation, and priority columns as described in the security model and data model.

**Security and auditability considerations:**

- Use Dataverse table permissions to ensure only authorised users can update exceptions.
- Keep resolution notes operational. Do not enter patient data, clinical observations, or real staff personal data in the demo.
- Use Dataverse auditing for the authoritative audit trail. The local collection is for demo visibility only and is cleared when the app session ends.
- For production, consider replacing direct `Patch` calls with a Power Automate flow if the action must trigger approvals, escalation emails, or additional audit records.

---

## Implementation Notes

- Test formulas with sample records covering each Priority, Status, Exception Type, and Trust value.
- Confirm delegation warnings in Power Apps Studio. Some formulas that traverse lookup columns or use `in` may need Dataverse views, indexed columns, or simplified predicates for large production datasets.
- Use consistent accessible labels on filter controls, action buttons, and gallery rows.
- Keep demo data fictional and workforce-operational. Do not use real NHS staff data or patient clinical data.
