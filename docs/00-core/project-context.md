# Project Context

## Purpose

BudgetApp is a personal budgeting web application used to manage monthly budgets, envelope budgets, income, expenses, recurring items, and reporting.

## Technology baseline

- ASP.NET Core 9 Razor Pages
- Entity Framework Core 9 with SQL Server
- Serilog for structured logging
- ClosedXML for Excel export
- Bootstrap 5, jQuery, and vanilla JavaScript on the client
- Session-based authentication
- Docker support for containerized execution

## Core domain model

- `Budget`: monthly time-bound budget or custom envelope budget
- `BudgetItem`: transaction recorded against a budget
- `BudgetItemLink`: additional inclusion of a primary item in other budgets
- `RecurringItem`: template used to copy recurring items into new monthly budgets
- `Category`: transaction classification
- `ItemName`: reusable item label
- `User`: session-authenticated owner of data

## Key runtime behaviors

- Monthly budgets are identified by `IsTimeBound = true`, `Month`, and two-digit `Year`.
- Transaction dates are persisted in UTC.
- The browser timezone is read from the `userTimeZone` cookie.
- When a new time-bound budget is created, all active recurring items are copied into it through service logic.
- Recurring items use the user's local calendar day for copying into a month-bound budget; if that day does not exist in the target month, the previous valid day is used.
- Quick Add on the home page resolves the primary monthly budget from the selected transaction date and MAY auto-create that budget.
- AJAX behavior is implemented through named Razor Page handlers instead of MVC controllers.

## Current repository shape

- Single main application project under `BudgetApp/`
- No dedicated test project currently present in the repository
- Existing documentation and agent rules live under `docs/` and `.github/copilot-instructions.md`

## Authoritative references

- Architecture: `../02-architecture/system-architecture.md`
- Structure: `../03-project-structure/project-structure.md`
- Coding rules: `../04-coding-standards/coding-standards.md`