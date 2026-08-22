# Database Architecture

## Authoritative scope

This file is the authoritative database-rules document for BudgetApp.

## Current persistence model

- SQL Server is the primary database.
- EF Core 9 is the persistence layer.
- `AppDbContext` is the single source of EF mapping configuration.
- Migrations live under `BudgetApp/Data/Migrations`.

## Database rules

- Model changes MUST be reflected in EF configuration and migrations.
- Query logic SHOULD remain in services.
- Ownership and user scoping MUST be preserved in queries.
- Delete behaviors MUST remain intentional and documented.