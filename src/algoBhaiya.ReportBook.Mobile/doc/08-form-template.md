# Form Template

## Business Rule

- The app can start a profile from a packaged planner template instead of a manually built form.
- Each preset has a name, category, summary, description, accent color, and ordered fields.
- A blank preset exists for users who want manual setup only.

## Startup Behavior

- After login, if the current profile has no active field templates, the app opens the planner catalog.
- The user can browse grouped templates, open a detail page, and review the included fields.
- Starting a non-blank preset creates the matching active field templates and current-month monthly targets for that profile.
- Starting the blank preset skips database seeding and returns to the main shell so the user can continue manual setup.

## UX Notes

- Show a loader while a preset is being prepared.
- Keep the catalog scrollable because the list can grow.
- Keep the detail page focused on the preset description and included fields.

## Source Of Truth

- `../algoBhaiya.ReportBook.Presentation/Views/PlannerTemplatePage.xaml`
- `../algoBhaiya.ReportBook.Presentation/Views/PlannerTemplateDetailPage.xaml`
- `../algoBhaiya.ReportBook.Presentation/Services/PlannerCatalogService.cs`
- `../algoBhaiya.ReportBook.MobileApp/Resources/Raw/planner-presets.json`
