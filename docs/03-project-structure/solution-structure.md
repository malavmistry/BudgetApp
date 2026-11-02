# Solution Structure

## Current solution model

The repository currently contains a single primary application project: `BudgetApp/BudgetApp.csproj`.

## Top-level repository items

- `.github/`: Copilot and repository-level automation guidance
- `docs/`: agentic documentation system
- `BudgetApp/`: ASP.NET Core application source
- `Dockerfile`: container build recipe
- `docker-compose.yml`: application container connected to the existing host SQL Server
- `.env.example`: non-secret template for Compose database settings; the actual `.env` is ignored
- `BudgetApp/appsettings.Local.json`: optional ignored local credentials/configuration, excluded from publishing and Docker build context

## Rule

Do not introduce a new project, package, or shared library unless the existing single-project structure becomes a documented constraint and the change is approved through architecture review.