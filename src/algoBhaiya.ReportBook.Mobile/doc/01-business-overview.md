# Business Overview

## Stable Facts

- The app is a .NET MAUI mobile app for offline daily reporting.
- All user data is stored locally in SQLite on the device.
- There is no remote backend in the current codebase.
- The solution is centered on multiple local profiles.
- The app is designed for repeated daily data capture and monthly review.
- The app also includes a local planner catalog that can seed a profile with predefined fields and monthly targets.

## Main Business Goal

- Let a user define what they track.
- Let the user start from a preset template or a blank template.
- Let the user enter daily values for those items.
- Let the user review monthly totals, averages, and completion status.
- Let the user manage several profiles on the same device.

## What The App Is Not

- It is not cloud-synced.
- It is not server-authenticated.
- It is not designed around shared accounts or remote collaboration.
- It is not backed by a remote template catalog; planner presets are packaged with the app.

## Where To Confirm

- Startup and registration: `../algoBhaiya.ReportBook.MobileApp/MauiProgram.cs`
- Navigation shell: `../algoBhaiya.ReportBook.MobileApp/AppShell.xaml.cs`
- Local storage and repositories: `../algoBhaiya.ReportBook.Infrastructure/Data/*`
