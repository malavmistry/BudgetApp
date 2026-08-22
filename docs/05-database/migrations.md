# Migrations

- Add a migration whenever a persistent model or EF configuration changes.
- Keep migrations in `BudgetApp/Data/Migrations`.
- Review generated migrations for unintended schema changes.
- Because the app applies migrations at startup, assess deployment safety before merge when migration risk is non-trivial.