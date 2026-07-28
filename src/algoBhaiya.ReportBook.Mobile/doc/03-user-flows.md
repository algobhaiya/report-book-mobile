# User Flows

## 1. Login And Profile Switching

- A user opens the app and lands on the login flow.
- The app can switch between profiles stored locally.
- Logout clears the current profile preference and returns to login.

## 2. Planner Template Start

- After login, if the current profile has no active field templates, the app opens the planner catalog.
- The user can browse grouped presets and open a detail view for each template.
- A non-blank preset seeds local field templates and current-month monthly targets for the active profile.
- A blank preset skips database work and returns to the main shell so the user can add fields manually.
- The planner start flow shows a loader while preset data is being prepared.

## 3. Field Setup

- The user configures field units first.
- The user then creates field templates that reference those units.
- Field templates define the day-to-day tracking items.
- Field order controls display order in the daily form and summary views.

## 4. Daily Entry

- The user opens the daily form for a selected date.
- Each field is filled according to its unit type.
- Saving persists the entry locally for that date and profile.
- Empty, zero, or false-like values are treated as invalid for storage in some save paths.

## 5. Monthly Target

- The user sets a monthly target per field template.
- The monthly plan can be opened for current or future months.
- The app can seed a default current-month plan from active templates when needed.

## 6. Monthly Summary And Calendar

- The monthly summary shows per-item totals, averages, and percentages.
- The filled-dates calendar shows which dates have entries.
- The summary can open into date-level detail for the selected month.

## 7. Settings

- The user can configure the edit-window duration for daily forms.
- The user can configure the local data-retention period.
- Cleanup runs incrementally so old data is removed in small batches.

## Where To Confirm

- Navigation and shells: `../algoBhaiya.ReportBook.MobileApp/AppShell.xaml.cs`
- Menu and profile actions: `../algoBhaiya.ReportBook.Presentation/ViewModels/AppShellViewModel.cs`
- Daily/monthly views: `../algoBhaiya.ReportBook.Presentation/Views/*`
