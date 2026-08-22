# System Architecture

## Authoritative scope

This file is the authoritative description of the current application architecture.

## Architectural style

BudgetApp is a server-rendered ASP.NET Core Razor Pages application with a service layer, EF Core persistence, and page-specific JavaScript for interactive flows.

## Primary layers

- `Pages`: request handling, session guard checks, model binding, response formatting
- `Services`: business rules and orchestration over persistence
- `Data`: EF Core context and persistence configuration
- `Models`: persistent entities
- `ViewModels`: UI and handler data contracts
- `wwwroot/js`: client behavior and AJAX calls

## Request flow

1. Browser loads Razor Page UI.
2. Client JavaScript optionally invokes named handlers using AJAX.
3. Page handler validates session and request input.
4. Handler calls a service.
5. Service reads or writes through `AppDbContext`.
6. Handler returns page content or JSON.

## Cross-cutting architecture rules

- Session-based auth is enforced at the page handler boundary.
- Business rules belong in services, not page handlers.
- EF relationships and indexes are configured centrally in `AppDbContext`.
- Frontend and handler contracts are tightly coupled and MUST stay aligned.
- Dates are converted using the user timezone cookie before persistence.

## Domain-specific architectural rules

- A budget item has one primary budget and zero or more linked budgets.
- Quick Add resolves the primary monthly budget from transaction date and must not ask the user to choose it.
- Recurring item behavior seeds future monthly budgets through service logic.
- Recurring item copying is based on the user's local calendar date semantics, not the stored UTC day.
- If a recurring item's local day is unavailable in the target month, service logic must clamp to the last valid day of that month.

## Runtime composition

`Program.cs` configures Razor Pages, SQL Server EF Core, session state, application services, Serilog, middleware, and automatic migration execution.