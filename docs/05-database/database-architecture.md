# Database Architecture

## Authoritative scope

This file is the authoritative database-rules document for BudgetApp.

## Current persistence model

- SQL Server is the primary database.
- EF Core 9 is the persistence layer.
- `AppDbContext` is the single source of EF mapping configuration.
- Migrations live under `BudgetApp/Data/Migrations`.
- Local execution connects to the host SQL Server at `localhost,1433`, database `BudgetAppDb`, using SQL authentication (`sa` for local development).
- Docker Compose runs only BudgetApp and connects to the same host SQL Server through `host.docker.internal,1433`; it does not provision SQL Server or manage its data.
- Non-secret defaults are in `BudgetApp/appsettings.json` under `Database` (`Host`, `Port`, `Name`, `User`). `Database:Password` MUST come from ignored local configuration or environment variables.
- Startup loads optional `BudgetApp/appsettings.Local.json`; environment variables and command-line arguments take precedence over that file. `ConnectionStrings:DefaultConnection`, when non-empty, overrides the component-based database settings for compatibility with existing deployments.
- Component-based connections use encryption and trust the local server certificate. For production, use a least-privileged login and a full connection-string override with certificate validation (`TrustServerCertificate=False`).
- Startup awaits `Database.MigrateAsync()` before accepting requests. It creates a missing database and applies pending migrations to an existing database; it does not drop or recreate existing data. Connection retries are enabled, and startup failures are logged and terminate with a non-zero exit code.

## Database rules

- Model changes MUST be reflected in EF configuration and migrations.
- Query logic SHOULD remain in services.
- Ownership and user scoping MUST be preserved in queries.
- Delete behaviors MUST remain intentional and documented.
- Recurring templates are stored in `RecurringItems`, owned by `User`, and `BudgetItems` reference them through an optional FK that must avoid SQL Server multi-cascade paths.