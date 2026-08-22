# Solution Structure

## Current solution model

The repository currently contains a single primary application project: `BudgetApp/BudgetApp.csproj`.

## Top-level repository items

- `.github/`: Copilot and repository-level automation guidance
- `docs/`: agentic documentation system
- `BudgetApp/`: ASP.NET Core application source
- `Dockerfile`: container build recipe
- `docker-compose.yml`: local multi-container runtime with SQL Server

## Rule

Do not introduce a new project, package, or shared library unless the existing single-project structure becomes a documented constraint and the change is approved through architecture review.