# Streak Details Popup Memory

## Purpose

The streak details popup is a lightweight summary view opened from the
Shell streak badge. It is meant to motivate daily report completion by
showing streak status and the last 7 days of filled activity.

## Entry Point

- The popup opens when the user taps the streak badge in `AppShell`.
- The badge shows the current streak count.
- The popup is powered by `AppShellViewModel` and uses the same selected user context as the rest of the app.

## Data Shown

### Current streak

- Display the current streak count in days.
- Display the next streak milestone in days.
- Display a progress bar toward the next milestone.

### Weekly activity

- Show the last 7 calendar days ending today.
- Each day shows the count of filled report items for that date.
- Highlight today.
- Empty days should still be shown with a muted visual state.

### Labels and copy

- Keep the popup focused on streak and weekly completion context.
- Use short motivational copy only if it does not add clutter.

## Milestones

- Use the milestone sequence currently supported by the app:
  `7, 14, 21, 30, 60, 90, 120, 180, 250, 365, 730, 1000`
- The next milestone is the first value greater than the current streak.
- Progress is calculated against that next milestone.

## Visual Rules

- The popup should support both light and dark mode.
- Use theme-aware colors instead of hardcoded dark-only colors.
- Keep the popup as a bottom sheet style modal with rounded corners.
- The header should stay simple and centered.

## Behavior

- Tapping outside the popup closes it.
- The popup should not open multiple times at once.
- Data should refresh when the popup opens so the weekly counts and streak are current.

## Implementation Notes

- `AppShellViewModel` owns the popup open command and the weekly data collection.
- `StreakWeekDayViewModel` lives in the `ViewModels` folder as a separate file.
- Weekly item values are derived from local SQLite data only.
- The app has no remote backend for this feature.

## Maintenance Notes

- Prefer the existing MVVM structure and keep popup-specific state out of the XAML code-behind when possible.
- Keep this document aligned with the actual popup behavior in code, not the original dashboard concept.
