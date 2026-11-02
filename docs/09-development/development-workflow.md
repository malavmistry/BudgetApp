# Development Workflow

- Start by loading `docs/README.md`, `99-meta/agent-rules.md`, and the relevant agent file.
- Prefer small iterative changes.
- Validate immediately after the first substantive edit.
- Update documentation when stable behavior or rules change.

## Local SQL Server setup

1. Start your existing SQL Server with TCP/IP and SQL authentication enabled on port `1433`.
2. In `BudgetApp/appsettings.Local.json` (ignored by Git), configure the password:

   ```json
   {
     "Database": {
       "Password": "<your-local-sql-password>"
     }
   }
   ```

3. Run from the repository root: `dotnet run --project BudgetApp\BudgetApp.csproj`.
4. The tracked defaults target `localhost,1433`, `BudgetAppDb`, and `sa`. Override `Database` values in the local file or via `Database__Host`, `Database__Port`, `Database__Name`, `Database__User`, and `Database__Password` environment variables.
5. Check startup logs for `Database migrations complete`. See [database architecture](../05-database/database-architecture.md) and [migrations](../05-database/migrations.md) for database lifecycle and permission requirements.

The local JSON file is excluded from publish output and Docker build context. Never put passwords in tracked settings, example files, logs, or documentation.

## Docker with the existing host SQL Server

1. Copy `.env.example` to `.env` in the repository root and set `DB_PASSWORD` to your local SQL Server password. `.env` is ignored by Git and excluded from the Docker build context.
2. Keep `DB_HOST=host.docker.internal`, `DB_PORT=1433`, `DB_NAME=BudgetAppDb`, and `DB_USER=sa` unless intentionally overriding them.
3. Validate configuration without displaying secrets: `docker compose config --quiet`.
4. Start the app: `docker compose up --build -d`.
5. Check `docker compose ps` and `docker compose logs budgetapp`; after migrations complete, open `http://localhost:8080`.
6. Stop the app: `docker compose down`. The host SQL Server and its data are not managed by Compose.

Do not use `localhost` as the database host inside the app container: it refers to that container, not Windows. Docker Desktop resolves `host.docker.internal`; Compose also includes the host-gateway mapping for compatible Docker Engine installations. The host SQL Server must accept TCP connections from Docker; enable the appropriate listener and firewall access if native startup works but container startup does not.

Native execution reads the local JSON file, not `.env`. Compose reads `.env` and injects `Database__*` variables into the container. When rotating the password, update both ignored files if you use both launch modes. Compose fails explicitly if `DB_PASSWORD` is absent or empty. Avoid unrestricted `docker compose config` output because it includes resolved credentials.

SQL Server backups are managed on the host (for example, through SSMS), not through a Compose SQL Server container. If upgrading from the former two-container setup, stop that stack first and back up/migrate any data in its `sqlserver-data` volume to the host server. Do not remove that volume until the move is verified; automatic app migrations do not transfer data between servers.