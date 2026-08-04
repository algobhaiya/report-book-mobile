# Floating Action Button

## Purpose

The app uses a floating action button on the two creation-heavy list pages:

- Units
- Items

The FAB replaces the old inline `Add` buttons and stays visible while the user scrolls the list.

## Current Implementation

- The shared FAB style lives in `MobileApp/App.xaml` as `FloatingActionButtonStyle`.
- The shared icon asset is `Presentation/Resources/Images/fab_add.svg`.
- `FieldUnitPage` and `FieldTemplatePage` both use an `ImageButton` anchored to the bottom-right of the page.
- Both pages reuse their existing add flows and still open the same add/edit modal pages.
- Both pages guard against rapid repeated taps with a local `_isAddModalOpen` flag.

## Visual Rules

- FAB size is `56x56`.
- Shape is a circle with `CornerRadius="28"`.
- Background uses the app primary color.
- The icon is a white plus sign rendered from SVG.
- The FAB uses a shadow/elevation effect and a pressed-state scale-down.
- Accessibility description is `Add new item`.

## Layout Notes

- The FAB is overlaid on top of the page content and does not take layout space.
- Bottom clearance is handled by the page container, not by adding a second action area.
- Both pages should keep the same overlay pattern so the list and FAB behave consistently.

## Maintenance Notes

- Prefer updating the shared `FloatingActionButtonStyle` if the FAB appearance changes.
- Keep the `fab_add.svg` asset in sync with the style rather than reintroducing text glyphs.
- If another creation-heavy list page is added later, reuse this same FAB pattern instead of inline `Add` buttons.
