# ADR-003: Use the host SQL Server for local and Docker execution

## Status

Accepted

## Context

Local development previously used LocalDB while Compose provisioned a separate SQL Server container. Docker database variables were not consumed by the app's connection-string registration.

## Problem

Native and container execution must connect to the user's existing SQL Server on host port `1433`, without provisioning another server or committing credentials.

## Decision

- Use the host SQL Server for both launch modes: `localhost` natively and `host.docker.internal` in Docker.
- Compose runs only the app; SQL Server lifecycle, persistence, and backups belong to the host.
- Keep non-secret database defaults in tracked settings; supply credentials through ignored local configuration or environment variables.
- Preserve a full `ConnectionStrings:DefaultConnection` override for existing deployments.
- Continue awaiting EF migrations at startup, including creation of a missing database. Startup failures must terminate rather than appear successful.

See [database architecture](../05-database/database-architecture.md) for the authoritative configuration contract and [development workflow](../09-development/development-workflow.md) for setup instructions.

## Alternatives Considered

- Keep a Compose-managed SQL Server: rejected by the requested existing-host-only deployment.
- Continue using LocalDB for native execution: would not share the requested SQL Server.
- Commit the supplied password: violates repository security standards.

## Consequences

- SQL Server must be running and reachable from the host and container before app startup can succeed.
- Migrating from the old Compose SQL Server requires an explicit backup/data transfer; EF migrations do not copy data between instances.
- Local credentials are not included in Git or application images; production deployments must supply their own secure configuration.

## Related Decisions

- [ADR-001](ADR-001-razor-pages-service-layer.md)

## Date

2026-11-01
