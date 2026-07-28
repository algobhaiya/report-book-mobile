# Settings And Gotchas

## Preferences Keys

- Use the constants in `Presentation.Constants.Constants`.
- Important keys include:
  - `CurrentUserId`
  - `Setting_ModificationDuration`
  - `Setting_DataRemovalPeriod`
  - `last_cleanup_date`
  - `IsSeedingInitialDataCompleted`
  - `Planner_BypassGate`

## Local Storage Rules

- Data lives in SQLite on the device.
- `Preferences` is used for lightweight app state.
- The app should not assume cloud sync or server persistence.

## Retention Behavior

- Retention cleanup runs incrementally.
- Old daily data is removed in small batches, not all at once.
- The cleanup marker is stored in preferences so repeated runs can resume safely.
- Targets and deleted templates are also cleaned with retention-aware rules.
- Planner preset startup can create or revive required local units, so it shares a semaphore gate with default unit seeding.

## Soft Delete Rules

- Several entities use `IsDeleted` instead of immediate deletion.
- Deleted templates can still affect summary behavior if there are existing entries.
- Monthly targets may be updated in place when the same user, template, month, and year already exist.
- If a user starts the blank planner preset, the app sets a bypass flag so the next shell load does not reopen the planner immediately.

## Known Codebase Gotchas

- There are a few namespace and reference inconsistencies in the solution.
- `DailyEntry.Value` is string-based even when the value is numeric or boolean.
- `FieldUnit.ValueType` and `FieldTemplate.ValueType` must stay aligned.

## Where To Confirm

- Constants: `../algoBhaiya.ReportBook.Presentation/Constants/Constants.cs`
- Retention service: `../algoBhaiya.ReportBook.Presentation/Services/DataRetentionService.cs`
- Seed data: `../algoBhaiya.ReportBook.Presentation/Services/SeedDataService.cs`
- Repository behavior: `../algoBhaiya.ReportBook.Infrastructure/Data/Repositories/*`
