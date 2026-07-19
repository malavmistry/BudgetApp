# BudgetApp – Copilot Instructions

## Project Overview

BudgetApp is a personal budget-tracking web application built with **ASP.NET Core 9 Razor Pages**, **Entity Framework Core** (SQL Server), **Serilog** for logging, and **ClosedXML** for Excel report export. The frontend uses **Bootstrap** and **jQuery** with vanilla JavaScript.

Users can create budgets (monthly time-bound or custom named envelopes), log income/expense transactions against them, mark items as recurring, link items across budgets, and generate reports with pie charts and Excel exports.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9.0, Razor Pages |
| ORM | EF Core 9 + SQL Server |
| Logging | Serilog (console + rolling file) |
| Excel Export | ClosedXML |
| Frontend | Bootstrap 5, jQuery, vanilla JS |
| Auth | Session-based (no ASP.NET Identity) |
| Container | Docker-ready (`DOTNET_RUNNING_IN_CONTAINER` env var) |

---

## Project Structure

```
BudgetApp/
  Constants/        # AppConstants, SessionKeys
  Data/             # AppDbContext + EF Migrations
  Enums/            # TransactionType (Expense=1, Earnings=2), ReportPeriod
  Models/           # EF entities: User, Budget, BudgetItem, BudgetItemLink, Category, ItemName
  Pages/            # Razor Pages (UI + named handler APIs)
  Services/         # Interface + implementation pairs for all business logic
  ViewModels/       # DTOs for pages and API responses
  wwwroot/js/       # Per-page JavaScript files (budget.js, category.js, home.js)
```

---

## Architecture & Key Patterns

### Service Layer
All business logic lives in scoped services registered in `Program.cs`. Always code against the interface, not the concrete class.

```csharp
// Correct
private readonly IBudgetService _budgetService;

// Every new service needs:
// 1. IMyService interface in Services/
// 2. MyService implementation in Services/
// 3. Registration in RegisterAppServices() in Program.cs
services.AddScoped<IMyService, MyService>();
```

### Razor Pages as API Endpoints
Pages use **named handlers** for AJAX calls instead of a separate controller layer. Handler method names follow the pattern `OnGet{Name}Async` / `OnPost{Name}Async`.

```csharp
// GET  /BudgetApi?handler=List
public async Task<IActionResult> OnGetListAsync() { ... }

// POST /BudgetApi?handler=SaveItem
public async Task<IActionResult> OnPostSaveItemAsync([FromBody] MyViewModel model) { ... }
```

Return `JsonResult` for API handlers. Return `Page()` or `RedirectToPage()` for UI handlers.

### Authentication
Auth is session-based. Always check session at the start of every handler:

```csharp
private int? CurrentUserId =>
    HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);

// In handlers:
if (!CurrentUserId.HasValue)
    return Unauthorized(); // for API handlers
    // or: return RedirectToPage("/Login"); // for page handlers
```

Never trust client-supplied user IDs — always use `CurrentUserId` from session.

### Timezone Handling
- All dates are **stored in UTC** in the database (`TransactionDateUtc`).
- The user's local timezone comes from the cookie `userTimeZone`.
- Convert to UTC before saving; convert back to local when displaying.

```csharp
var timeZoneId = Request.Cookies["userTimeZone"] ?? "UTC";
var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
var utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, timeZone);
```

### Date Format
Use `AppConstants.DATE_FORMAT` (`"MM-dd-yyyy"`) consistently for parsing/formatting transaction dates.

---

## Domain Model

### Budget
Two types, controlled by `IsTimeBound`:
- **Time-bound** (`IsTimeBound = true`): monthly budget, e.g. "July 26". Has `Month` (1–12) and `Year` (2-digit, e.g. `26` for 2026).
- **Envelope** (`IsTimeBound = false`): custom named budget, e.g. "Home Expenses". `Month`/`Year` are null.

When saving a `BudgetItem` to an envelope, a corresponding time-bound budget for that month is auto-created via `EnsureTimeBoundBudgetAsync()` and linked.

### BudgetItem
A single income or expense transaction. Key fields:
- `Type`: `TransactionType.Expense` (1) or `TransactionType.Earnings` (2)
- `Amount`: stored with precision `decimal(18,2)`, truncated to 2 decimal places (not rounded)
- `IsRecurring`: recurring items are copied to new monthly budgets automatically
- `TransactionDateUtc`: always UTC
- `ItemNameId` / `CategoryId`: FK references, delete behavior is `Restrict`

### BudgetItemLink
Links a `BudgetItem` to additional budgets (many-to-many via link table). A unique constraint exists on `(BudgetItemId, LinkedBudgetId)`. Use a `HashSet<int>` to deduplicate desired links before inserting.

---

## Constants Reference

```csharp
AppConstants.NAME_MAX_LENGTH         // 25
AppConstants.DESCRIPTION_MAX_LENGTH  // 200
AppConstants.NOTE_MAX_LENGTH         // 500
AppConstants.DATE_FORMAT             // "MM-dd-yyyy"

SessionKeys.LOGGED_IN_USER_ID        // "LoggedInUserId"
SessionKeys.LOGGED_IN_USERNAME       // "LoggedInUsername"
```

---

## Database & Migrations

- Migrations are applied automatically on startup via `ApplyMigrationsAsync()`.
- When changing a model, always add a migration:
  ```
  dotnet ef migrations add <MigrationName> --project BudgetApp
  ```
- Configure entity relationships in `AppDbContext.OnModelCreating()` using private static `Configure{Entity}` methods — one per entity.
- Cascade delete: `Budget → BudgetItems → BudgetItemLinks`. Use `DeleteBehavior.Restrict` for `ItemName` and `Category` references.

---

## Logging

Use `ILogger<T>` injected via constructor. Follow structured logging with named parameters:

```csharp
_logger.LogInformation("Created budget {BudgetName} for user {UserId}", name, userId);
_logger.LogWarning("Budget {BudgetId} not found for user {UserId}", budgetId, userId);
```

---

## Frontend Conventions

- Each Razor Page has a corresponding JS file in `wwwroot/js/` (e.g. `budget.js` for the Budget page).
- API calls use jQuery `$.ajax()` or `fetch`.
- The user's timezone is captured from the browser and stored in the `userTimeZone` cookie, then read server-side on every request.
- `site.js` contains shared utilities.

---

## Adding New Features – Checklist

1. **Model change?** → Update `AppDbContext`, add EF migration.
2. **New service operation?** → Add to the interface first, then implement.
3. **New page?** → Create `.cshtml` + `.cshtml.cs`, add session auth guard.
4. **New API handler?** → Named handler on an existing `*Api.cshtml.cs` page; return `JsonResult`.
5. **New JS interaction?** → Add to the relevant per-page JS file in `wwwroot/js/`.
6. **New constant?** → Add to `AppConstants` or `SessionKeys`; never use magic strings/numbers inline.
