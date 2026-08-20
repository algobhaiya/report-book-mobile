# Architecture And Navigation

## Project Boundaries

- `Core`
  - Entities, DTOs, and repository or service interfaces.
- `Infrastructure`
  - SQLite repositories and database initialization.
- `Presentation`
  - View models, views, services, helpers, constants, and UI-facing logic.
- `MobileApp`
  - MAUI host and dependency-injection composition root.

## Startup Flow

- `MauiProgram.cs` creates the app and registers services.
- SQLite uses a local file under app data.
- `DatabaseInitializer` creates the core tables on startup.
- Fonts, pages, view models, and services are registered in DI.
- `SeedDataService` seeds default units, while `PlannerCatalogService` loads packaged presets and ensures any required units exist.

## Navigation Flow

- `AppNavigator` is the central navigation abstraction.
- `AppShell` is the main shell after login.
- `NavigationPage` is used for the login page.
- `NavigateToPlanner()` replaces the main page with the planner catalog when a profile has no active fields.
- Shell routes exist for daily entry, monthly summary, settings, and switch profile.
- The shell-hosted daily and monthly summary pages are opened through `Shell.Current.GoToAsync(...)` so Android back returns to the previous page instead of exiting the app.
- Some flows use modal pages, especially menu and profile switching.
- `AppShellViewModel` redirects a logged-in profile to the planner catalog unless the planner bypass gate has been set by the blank-template flow.

## Important Runtime Notes

- Shell-based navigation depends on `Shell.Current`.
- Modal flows are used alongside routed navigation.
- Navigation data is passed through an in-memory helper when needed.
- The planner catalog uses its own `NavigationPage` flow so the user can open a template detail page before starting a plan.

## Where To Confirm

- App startup: `../algoBhaiya.ReportBook.MobileApp/MauiProgram.cs`
- Shell: `../algoBhaiya.ReportBook.MobileApp/AppShell.xaml.cs`
- Navigator: `../algoBhaiya.ReportBook.MobileApp/Services/AppNavigator.cs`
- App shell state: `../algoBhaiya.ReportBook.Presentation/ViewModels/AppShellViewModel.cs`
