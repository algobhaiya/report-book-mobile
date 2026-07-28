# Feature Map

## Login And Profiles

- `LoginPage` starts the app flow for local users.
- `SwitchProfilePage` lets the user change profiles without a backend.
- `SwitchProfilePageViewModel` loads non-deleted profiles and updates `CurrentUserId`.
- `AppShellViewModel.LogoutAsync` clears the current profile and returns to login.

## Field Units

- `FieldUnitPage` lists available units.
- `FieldUnitAddEditPage` edits a unit in a modal flow.
- `FieldUnitAddEditViewModel` validates name and display type, then soft-deletes the old unit and saves the replacement.
- Unit edits may update related field templates so templates keep pointing to the active unit.

## Field Templates

- `FieldTemplatePage` lists tracked items for the current profile.
- `FieldTemplateDetailPage` opens add/edit details.
- `FieldTemplateDetailViewModel` validates field name and unit, then applies soft-delete plus replacement save behavior.
- Template order controls the daily form and monthly target ordering.

## Planner Templates

- `PlannerTemplatePage` shows grouped packaged presets before the main shell when a profile has no active fields.
- `PlannerTemplateDetailPage` shows the preset description and included fields.
- `PlannerCatalogService` loads the packaged catalog, ensures required units exist, and seeds profile field templates plus current-month targets for non-blank presets.
- Blank presets skip the seed step and route the user directly into the daily entry shell.

## Daily Entry

- `DailyEntryPage` is the main daily form.
- `DailyEntryViewModel` loads entries for the selected date and current profile.
- The view model respects the edit-window preference before enabling submit.
- `DailyEntryListPage` shows dates for a month and can open a form by day, date picker, or month picker.

## Monthly Target And Summary

- `MonthlyTargetPage` captures monthly plan values.
- `MonthlyTargetViewModel` loads targets for the selected month and current profile.
- The view model switches between read-only history and editable current/future month behavior.
- `MonthlySummaryPage` shows monthly totals, averages, and percentages.
- `MonthlySummaryViewModel` can open the filled-dates calendar for an item.

## Settings And Support

- `SettingsPage` exposes edit-window and data-retention preferences.
- `HelpPage` holds in-app guidance.
- `MenuSheetPage` drives the shell menu actions.
- `AppShellViewModel` owns menu, settings, summary, profile switch, and logout navigation.

## Supporting Services

- `SeedDataService` seeds default units on first run.
- `FieldUnitSeedGate` prevents the default unit seed and planner unit seed from racing each other.
- `DataRetentionService` removes old local data incrementally.
- `NavigationDataService` passes temporary state between pages and modal flows.
- `AppNavigator` switches between login, shell, routed pages, and modal pages.

## Where To Confirm

- `../algoBhaiya.ReportBook.MobileApp/MauiProgram.cs`
- `../algoBhaiya.ReportBook.Presentation/ViewModels/*`
- `../algoBhaiya.ReportBook.Presentation/Views/*`
- `../algoBhaiya.ReportBook.Presentation/Services/*`
- `../algoBhaiya.ReportBook.MobileApp/Services/AppNavigator.cs`
