# Integration Patterns

## Current patterns

- Named Razor Page handlers act as the JSON API surface.
- Client-side interactions use jQuery, `fetch`, or project helpers from `site.js`.
- Services integrate with SQL Server via EF Core.
- Reports integrate with ClosedXML for file export.
- Logging integrates through `ILogger<T>` and Serilog sinks.

## Rules

- Keep transport concerns in handlers.
- Keep persistence and business rules in services.
- Keep client payload shape consistent with the handler response.