# Canvas App Screens

## App Overview

**App name:** NHSP Shift Exception Manager
**Type:** Canvas app (tablet layout, 16:9)
**Navigation:** Left-hand sidebar with icon navigation

The app is designed for operations coordinators to triage shift exceptions from a single, focused interface. It follows a master-detail pattern to minimise screen switching during the demo.

---

## Screen 1: Exception Dashboard (Home)

**Purpose:** Primary working screen. Displays all open and in-progress exceptions in a filterable list with a detail panel.

### Layout

```
┌──────────┬──────────────────────────────────────────────────┐
│          │  Header: "Shift Exception Dashboard"    [User]   │
│  Nav     ├──────────────────────────────────────────────────┤
│  Bar     │  Filter Bar                                      │
│          │  [Priority ▼] [Type ▼] [Status ▼] [Trust ▼]      │
│          ├────────────────────────┬─────────────────────────┤
│          │  Exception List        │  Exception Detail       │
│          │                        │                         │
│          │  EXC-3001              │  Exception ID: EXC-3001 │
│          │  Unfilled Shift | High │  Shift: SFT-1001        │
│          │  Northshire | Open     │  Trust: Northshire      │
│          │  ───────────────────   │  Ward: Emergency Dept   │
│          │  EXC-3002              │  Type: Unfilled Shift   │
│          │  Compliance | Critical │  Priority: High         │
│          │  Northshire | Open     │  Status: Open           │
│          │  ───────────────────   │  Escalation: Not Esc.   │
│          │  EXC-3003              │  ─────────────────────  │
│          │  Urgent | High         │  [Change Status ▼]      │
│          │  Southvale | In Prog   │  [Escalate]  [Resolve]  │
│          │                        │  Resolution Notes: ___  │
└──────────┴────────────────────────┴─────────────────────────┘
```

### Key Controls

| Control | Type | Data Source | Notes |
|---|---|---|---|
| Priority filter | Dropdown | Priority choice values | Filters exception gallery |
| Type filter | Dropdown | Exception Type choice values | Filters exception gallery |
| Status filter | Dropdown | Status choice values | Defaults to "Open, In Progress" |
| Trust filter | Dropdown | Trust choice values via related Shift | Filters by trust |
| Exception gallery | Vertical gallery | `nhsp_ShiftException` | Sorted by Priority (Critical first), then CreatedTime |
| Detail panel | Form (view/edit) | Selected exception record | Displays full details with related shift info |
| Change Status | Dropdown | Status choice values | Updates `nhsp_Status` on the selected record |
| Escalate button | Button | — | Sets `nhsp_EscalationStatus` to "Escalated" and triggers escalation flow |
| Resolve button | Button | — | Sets `nhsp_Status` to "Resolved" and `nhsp_ResolvedTime` to `Now()` |

### Key Power Fx

```
// Gallery Items with filters
Filter(
    nhsp_ShiftExceptions,
    (drpPriority.Selected.Value = "All" Or nhsp_Priority = drpPriority.Selected.Value) &&
    (drpType.Selected.Value = "All" Or nhsp_ExceptionType = drpType.Selected.Value) &&
    (drpStatus.Selected.Value = "All" Or nhsp_Status = drpStatus.Selected.Value)
)
```

---

## Screen 2: Shift Overview

**Purpose:** Shows all upcoming shifts with fill status, helping coordinators spot gaps before exceptions are raised.

### Layout

```
┌──────────┬──────────────────────────────────────────────────┐
│          │  Header: "Shift Overview"               [User]   │
│  Nav     ├──────────────────────────────────────────────────┤
│  Bar     │  Filter: [Trust ▼] [Date ▼]                      │
│          ├──────────────────────────────────────────────────┤
│          │  Shift Gallery                                   │
│          │  ┌──────────────────────────────────────────┐    │
│          │  │ SFT-1001 | Northshire | Emergency Dept   │    │
│          │  │ RN | 06 May 07:00–19:00 | 3/4 Filled ██░ │    │
│          │  ├──────────────────────────────────────────┤    │
│          │  │ SFT-1002 | Northshire | Acute Medical    │    │
│          │  │ HCA | 06 May 08:00–16:00 | 2/3 Filled █░░│    │
│          │  ├──────────────────────────────────────────┤    │
│          │  │ SFT-1003 | Southvale | Paediatrics       │    │
│          │  │ RN | 06 May 12:00–20:00 | 2/2 Filled ███ │    │
│          │  └──────────────────────────────────────────┘    │
└──────────┴──────────────────────────────────────────────────┘
```

### Key Controls

| Control | Type | Notes |
|---|---|---|
| Shift gallery | Vertical gallery | Colour-coded fill bar: green (filled), amber (partially filled), red (empty) |
| Fill indicator | Progress bar | `nhsp_FilledWorkers / nhsp_RequiredWorkers` |
| Conditional formatting | — | Row background turns red if `nhsp_Status = "Open"` and shift starts within 12 hours |

---

## Screen 3: Worker Availability

**Purpose:** Quick-reference screen showing bank workers, their compliance status, and availability. Used when coordinators need to find a replacement worker.

### Layout

```
┌──────────┬────────────────────────────────────────────────────┐
│          │  Header: "Worker Availability"          [User]     │
│  Nav     ├────────────────────────────────────────────────────┤
│  Bar     │  Filter: [Role ▼] [Compliance ▼] [Trust ▼]         │
│          ├────────────────────────────────────────────────────┤
│          │  Worker Gallery                                    │
│          │  ┌──────────────────────────────────────────────┐  │
│          │  │ WRK-2001 | Aisha Khan | RN                   │  │
│          │  │ ✅ Compliant | Available | Northshire        │  │
│          │  ├──────────────────────────────────────────────┤  │
│          │  │ WRK-2002 | Daniel Hughes | HCA               │  │
│          │  │ ⚠️ Training Expired | Available | Northshire │  │
│          │  ├──────────────────────────────────────────────┤  │
│          │  │ WRK-2004 | Thomas Green | HCA                │  │
│          │  │ ✅ Compliant | Available | Southvale         │  │
│          │  └──────────────────────────────────────────────┘  │
└──────────┴────────────────────────────────────────────────────┘
```

### Key Controls

| Control | Type | Notes |
|---|---|---|
| Worker gallery | Vertical gallery | Filtered by role, compliance status, and preferred trust |
| Compliance icon | Icon control | Green tick for Compliant, amber warning for non-compliant states |
| Availability badge | Label | Green background for Available, grey for Unavailable |

---

## Screen 4: Exception Detail / Edit

**Purpose:** Full-page edit form for a single exception, accessible by selecting a record from the dashboard and tapping "View Full Details". Used for adding resolution notes and reviewing related shift context.

### Layout

```
┌──────────┬──────────────────────────────────────────────────┐
│          │  Header: "Exception Detail"     [Back] [Save]    │
│  Nav     ├──────────────────────────────────────────────────┤
│  Bar     │  Exception Form                                  │
│          │  Exception ID:      EXC-3001                     │
│          │  Exception Type:    Unfilled Shift               │
│          │  Priority:          [High ▼]                     │
│          │  Status:            [Open ▼]                     │
│          │  Escalation:        [Not Escalated ▼]            │
│          │  ────────────────────────────────────────────    │
│          │  Related Shift                                   │
│          │  Shift ID: SFT-1001 | Trust: Northshire          │
│          │  Ward: Emergency Dept | RN | 07:00–19:00         │
│          │  Fill: 3 / 4                                     │
│          │  ────────────────────────────────────────────    │
│          │  Resolution Notes:                               │
│          │  [                                         ]     │
│          │  Resolved Time:     [Date picker]                │
│          │  ────────────────────────────────────────────    │
│          │  [Escalate]   [Resolve]   [Cancel]               │
└──────────┴──────────────────────────────────────────────────┘
```

---

## Accessibility Considerations

| Consideration | Implementation |
|---|---|
| **Colour contrast** | All text meets WCAG 2.1 AA contrast ratios (4.5:1 minimum). Status colours are paired with icons/text labels — never colour alone. |
| **Screen reader support** | All controls have `AccessibleLabel` properties set. Gallery items include descriptive labels (e.g. "Exception EXC-3001, Unfilled Shift, High priority, Open status"). |
| **Keyboard navigation** | Tab order is set logically across filter bar → gallery → detail panel. Action buttons are keyboard-accessible. |
| **Text scaling** | Font sizes use relative sizing; layout accommodates up to 200% zoom without horizontal scrolling. |
| **Focus indicators** | Visible focus borders on all interactive controls. |

## Demo Flow (35-minute Power Platform segment)

This walkthrough supports the Power Platform portion of the wider 45-minute demo. The first 10 minutes are reserved for the Product Manager agent creating GitHub Issues for the delivery backlog.

| Step | Screen | Action | Duration |
|---|---|---|---|
| 1 | Exception Dashboard | Show filtered view of open exceptions, explain triage process | 7 min |
| 2 | Exception Detail | Walk through triaging EXC-3001, change status, add notes | 6 min |
| 3 | Shift Overview | Show shift fill status, highlight gap that caused the exception | 5 min |
| 4 | Worker Availability | Find available compliant worker for the unfilled shift | 5 min |
| 5 | Exception Dashboard | Escalate EXC-3002 (compliance blocker), show Power Automate trigger | 7 min |
| 6 | Admin or documentation view | Show Dataverse audit log, security roles, and readiness review | 5 min |
