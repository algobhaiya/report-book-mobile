# Domain Model

## Core Entities

- `AppUser`
  - Local profile record.
  - Uses simplified local login fields.
  - Supports soft delete through `IsDeleted`.
- `FieldUnit`
  - Describes how a value should be displayed and interpreted.
  - Example unit names include `Hours`, `Minutes`, `Checkbox`, `Pages`, `Ayat`.
  - `ValueType` must stay aligned with field handling.
- `FieldTemplate`
  - Defines one tracked item for a profile.
  - Stores `FieldName`, `UnitId`, `FieldOrder`, `IsEnabled`, and `IsDeleted`.
- `PlannerPreset`
  - Defines a packaged plan template shown before the daily form.
  - Stores name, category, summary, description, accent color, sort order, and `IsBlank`.
  - `IsBlank` means the preset does not seed fields and only routes the user into manual setup.
- `PlannerPresetField`
  - Defines one field inside a preset template.
  - Stores field name, unit, value type, and display order.
- `DailyEntry`
  - Stores one saved value for one template on one date.
  - `Value` is stored as a string and interpreted by unit type.
- `MonthlyTarget`
  - Stores a monthly target for a profile and field template.
  - Contains month, year, target value, and display order.

## Summary Shapes

- `DailySummaryItem`
  - Used to show filled-day counts.
  - Tracks date, filled count, total fields, and completion status.
- `MonthlySummaryItem`
  - Used to show item-level monthly reporting.
  - Tracks item name, unit, days filled, sum, average, percentage, and filled dates.

## Model Rules

- `FieldUnit.ValueType` and `FieldTemplate.ValueType` should stay consistent.
- `PlannerPresetField.ValueType` must stay aligned with the unit that gets seeded for that preset.
- `DailyEntry.Value` is not strongly typed in storage.
- Many records use `IsDeleted` instead of immediate hard deletion.
- Planner presets are loaded from packaged JSON and mapped to local units before the user starts them.

## Where To Confirm

- Entities: `../algoBhaiya.ReportBook.Core/Entities/*`
- DTOs: `../algoBhaiya.ReportBook.Core/Dtos/*`
- Reporting logic: `../algoBhaiya.ReportBook.Infrastructure/Data/Repositories/DailyEntryRepository.cs`
