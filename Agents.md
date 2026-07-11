# algoBhaiya.ReportBook.Mobile

## Overview

`algoBhaiya.ReportBook.Mobile` is a .NET MAUI mobile app for offline daily reporting.
All user data is stored locally in SQLite on the device, with no remote backend in the current codebase.

The solution root is:

`D:\Projects\algoBhaiya.ReportBook\src\algoBhaiya.ReportBook.Mobile`

Main projects:

- `algoBhaiya.ReportBook.MobileApp`
- `algoBhaiya.ReportBook.Presentation`
- `algoBhaiya.ReportBook.Infrastructure`
- `algoBhaiya.ReportBook.Core`

## Architecture

- `Core` contains entities, DTOs, and repository/service interfaces.
- `Infrastructure` implements SQLite repositories and database initialization.
- `Presentation` contains view models, views, converters, helper services, and UI-facing constants.
- `MobileApp` is the MAUI host and dependency-injection composition root.

## Startup And Navigation

- `MobileApp/MauiProgram.cs` wires up `SQLiteAsyncConnection`, repositories, services, view models, pages, and fonts.
- `MobileApp/AppShell.xaml.cs` is the main shell after login and registers routes for monthly summary, settings, and profile switching.
- `MobileApp/Services/AppNavigator.cs` is the central navigation abstraction for shell switching, login switching, routed navigation, and modal pages.
- `Presentation/ViewModels/AppShellViewModel.cs` drives the shell menu and login/logout/profile navigation behavior.

## Data Model Conventions

- `DailyEntry.Value` stores the value as a string and is interpreted according to the field unit type.
- `MonthlyTarget` stores month/year targets per user and field template.
- Several entities use `IsDeleted` for soft-delete behavior rather than immediate hard removal.
- `FieldUnit` defines the display unit and its value type, which must stay aligned with field template value handling.

## Major User Flows

- Login, create, or select a profile.
- Configure field units and field templates.
- Enter daily data for the selected date.
- Define monthly targets.
- Review monthly summary and open the filled-dates calendar.
- Adjust settings for edit-window duration and data-retention period.

## Maintenance Rules And Gotchas

- Prefer `Preferences` keys defined in `Presentation.Constants.Constants`.
- Do not assume cloud sync, remote auth, or server-side persistence.
- Data retention is incremental and deletes older local daily data in small batches.
- Navigation depends on `Shell.Current`, `NavigationPage`, and modal popups in several places.
- There are a few namespace and reference inconsistencies in the current codebase, so be careful when adding new cross-project imports.

## Where To Look First

- Startup: `algoBhaiya.ReportBook.MobileApp/MauiProgram.cs`
- Navigation: `algoBhaiya.ReportBook.MobileApp/Services/AppNavigator.cs`, `algoBhaiya.ReportBook.Presentation/ViewModels/AppShellViewModel.cs`
- Data access: `algoBhaiya.ReportBook.Infrastructure/Data/Repositories/*`
- Settings and retention: `algoBhaiya.ReportBook.Presentation/Services/*`
- Main feature pages: `algoBhaiya.ReportBook.Presentation/Views/*`

## Notes For Future Work

- Treat this file as a living context guide for the solution.
- When answering future questions, use the code in this solution as the source of truth rather than assumptions from MAUI conventions.
