# Migrations

- Add a migration whenever a persistent model or EF configuration changes.
- Keep migrations in `BudgetApp/Data/Migrations`.
- Review generated migrations for unintended schema changes.
- Because the app applies migrations at startup, assess deployment safety before merge when migration risk is non-trivial.
- Startup uses `MigrateAsync`, not `EnsureCreated`: a missing database is created from the migration chain, while an existing EF-managed database receives only pending migrations.
- The configured login needs permission to create the database when absent and apply schema changes when present. `sa` is intended only for the requested local setup; use a separate migration/deployment login and least-privileged runtime access in production.
- An existing database created outside the EF migration chain requires an explicit baseline/import plan; startup does not infer migration history from tables.
- Connection/configuration changes alone do not require a new schema migration.