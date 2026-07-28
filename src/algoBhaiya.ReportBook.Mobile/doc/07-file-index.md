# File Index

## Solution And Startup

- `../algoBhaiya.ReportBook.Mobile.sln` - solution root for the four-project app.
- `../algoBhaiya.ReportBook.MobileApp/MauiProgram.cs` - DI composition root and app startup.
- `../algoBhaiya.ReportBook.MobileApp/AppShell.xaml.cs` - main shell and route registration.
- `../algoBhaiya.ReportBook.MobileApp/Services/AppNavigator.cs` - central navigation abstraction.
- `../algoBhaiya.ReportBook.MobileApp/Resources/Raw/planner-presets.json` - packaged planner catalog definitions.

## Core Domain

- `../algoBhaiya.ReportBook.Core/Entities/AppUser.cs` - local profile record.
- `../algoBhaiya.ReportBook.Core/Entities/FieldUnit.cs` - unit definition and value type.
- `../algoBhaiya.ReportBook.Core/Entities/FieldTemplate.cs` - tracked item definition.
- `../algoBhaiya.ReportBook.Core/Entities/PlannerPreset.cs` - packaged planner template definition.
- `../algoBhaiya.ReportBook.Core/Entities/PlannerPresetField.cs` - field inside a planner template.
- `../algoBhaiya.ReportBook.Core/Entities/DailyEntry.cs` - stored daily value.
- `../algoBhaiya.ReportBook.Core/Entities/MonthlyTarget.cs` - per-month target definition.
- `../algoBhaiya.ReportBook.Core/Entities/MonthlySummaryItem.cs` - summary row for the monthly report.
- `../algoBhaiya.ReportBook.Core/Dtos/DailySummaryItem.cs` - filled-day summary for the calendar view.
- `../algoBhaiya.ReportBook.Core/Interfaces/*` - repository and service contracts.
- `../algoBhaiya.ReportBook.Core/Interfaces/IPlannerCatalogService.cs` - planner catalog loading and seeding contract.
- `../algoBhaiya.ReportBook.Core/Interfaces/IAppNavigator.cs` - navigation contract including planner entry.

## Infrastructure

- `../algoBhaiya.ReportBook.Infrastructure/Data/DatabaseInitializer.cs` - creates SQLite tables.
- `../algoBhaiya.ReportBook.Infrastructure/Data/Repositories/Repository.cs` - generic CRUD wrapper.
- `../algoBhaiya.ReportBook.Infrastructure/Data/Repositories/DailyEntryRepository.cs` - daily entry save and reporting queries.
- `../algoBhaiya.ReportBook.Infrastructure/Data/Repositories/MonthlyTargetRepository.cs` - target save and lookup logic.

## Presentation Constants And Helpers

- `../algoBhaiya.ReportBook.Presentation/Constants/Constants.cs` - shared preference keys and navigation keys.
- `../algoBhaiya.ReportBook.Presentation/Helpers/NavigationDataService.cs` - transient in-memory state passing.
- `../algoBhaiya.ReportBook.Presentation/Helpers/BoolStringConverter.cs` - UI converter for boolean text.
- `../algoBhaiya.ReportBook.Presentation/Helpers/FieldTemplateSelector.cs` - template-based view selection.
- `../algoBhaiya.ReportBook.Presentation/Helpers/ValueTypeToVisibilityConverter.cs` - UI visibility conversion by unit type.

## Presentation Services

- `../algoBhaiya.ReportBook.Presentation/Services/SeedDataService.cs` - seeds default units on first run.
- `../algoBhaiya.ReportBook.Presentation/Services/PlannerCatalogService.cs` - loads planner presets and starts selected plans.
- `../algoBhaiya.ReportBook.Presentation/Services/FieldUnitSeedGate.cs` - semaphore gate for unit seeding coordination.
- `../algoBhaiya.ReportBook.Presentation/Services/DataRetentionService.cs` - incremental cleanup of old local data.

## Presentation ViewModels

- `../algoBhaiya.ReportBook.Presentation/ViewModels/AppShellViewModel.cs` - shell menu, logout, and navigation.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/DailyEntryViewModel.cs` - daily form loading and submit flow.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/DailyEntryListViewModel.cs` - month/day list and search flow.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/DailyEntrySummaryViewModel.cs` - daily entry summary support.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/FieldTemplateDetailViewModel.cs` - add/edit field templates.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/FieldUnitAddEditViewModel.cs` - add/edit field units.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/PlannerPresetGroup.cs` - grouped preset collection for the planner catalog.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/FilledDatesCalendarViewModel.cs` - filled-date calendar support.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/MonthlySummaryViewModel.cs` - monthly report and calendar navigation.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/MonthlyTargetViewModel.cs` - monthly planning and edit behavior.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/SettingsViewModel.cs` - settings and retention preferences.
- `../algoBhaiya.ReportBook.Presentation/ViewModels/SwitchProfilePageViewModel.cs` - local profile switching.

## Presentation Views

- `../algoBhaiya.ReportBook.Presentation/Views/LoginPage.xaml` - login UI.
- `../algoBhaiya.ReportBook.Presentation/Views/MenuSheetPage.xaml` - shell menu modal.
- `../algoBhaiya.ReportBook.Presentation/Views/FieldUnitPage.xaml` - units list.
- `../algoBhaiya.ReportBook.Presentation/Views/FieldUnitAddEditPage.xaml` - unit edit modal.
- `../algoBhaiya.ReportBook.Presentation/Views/FieldTemplatePage.xaml` - templates list.
- `../algoBhaiya.ReportBook.Presentation/Views/FieldTemplateDetailPage.xaml` - template edit modal.
- `../algoBhaiya.ReportBook.Presentation/Views/PlannerTemplatePage.xaml` - planner catalog list.
- `../algoBhaiya.ReportBook.Presentation/Views/PlannerTemplateDetailPage.xaml` - planner preset detail page.
- `../algoBhaiya.ReportBook.Presentation/Views/DailyEntryPage.xaml` - daily form.
- `../algoBhaiya.ReportBook.Presentation/Views/DailyEntryListPage.xaml` - daily entry calendar/list.
- `../algoBhaiya.ReportBook.Presentation/Views/MonthlyTargetPage.xaml` - monthly planning.
- `../algoBhaiya.ReportBook.Presentation/Views/MonthlySummaryPage.xaml` - monthly report.
- `../algoBhaiya.ReportBook.Presentation/Views/FilledDatesCalendarPage.xaml` - filled-date calendar modal.
- `../algoBhaiya.ReportBook.Presentation/Views/SettingsPage.xaml` - settings screen.
- `../algoBhaiya.ReportBook.Presentation/Views/SwitchProfilePage.xaml` - profile switch modal.
- `../algoBhaiya.ReportBook.Presentation/Views/HelpPage.xaml` - help screen.
- `../algoBhaiya.ReportBook.Presentation/Views/DatePickerPopup.xaml` - date picker popup.
- `../algoBhaiya.ReportBook.Presentation/Views/YearMonthPickerPopup.xaml` - month picker popup.

## Note

- This index is intentionally shallow.
- Use the feature map and the source tree together for deeper implementation work.
