# Project Structure

## Authoritative scope

This file is the authoritative map of where work belongs in the repository.

## Application structure

- `BudgetApp/Constants`: shared constants and session keys
- `BudgetApp/Data`: `AppDbContext` and EF Core migrations
- `BudgetApp/Enums`: domain enums
- `BudgetApp/Models`: EF entities
- `BudgetApp/Pages`: UI pages and named-handler endpoints
- `BudgetApp/Properties`: launch configuration
- `BudgetApp/Services`: service interfaces and implementations
- `BudgetApp/ViewModels`: page and API contract models
- `BudgetApp/wwwroot/css`: site styling
- `BudgetApp/wwwroot/js`: page and shared JavaScript
- `BudgetApp/wwwroot/lib`: third-party frontend libraries

## Placement rules

- Add business behavior to `Services`.
- Add request or page orchestration to `Pages`.
- Add persistent entity changes to `Models` and `Data`.
- Add EF configuration changes to `Data/AppDbContext.cs`.
- Add request and response shapes to `ViewModels` when they are reused or transport-specific.
- Add shared literals to `Constants`.

## Important anchors

- `Program.cs`: application composition root
- `Pages/BudgetApi.cshtml.cs`: central budget and item handler surface
- `Services/BudgetService.cs`: central budget business logic
- `wwwroot/js/budget.js`: primary interactive budget client module