# Folder Organization

## Rules

- Keep interface and implementation files together in `Services` unless an accepted restructure says otherwise.
- Keep page `.cshtml` and `.cshtml.cs` pairs adjacent.
- Keep API-like handlers in clearly named page model files such as `BudgetApi.cshtml.cs`.
- Keep page-specific scripts in `wwwroot/js` with names that match the page or feature area.
- Do not create generic dumping-ground folders.

## Examples

- A new budget-specific client flow belongs in `wwwroot/js/budget.js` or a nearby budget-focused script.
- A new category service method belongs in `ICategoryService.cs` and `CategoryService.cs`.