# ADR-001: Razor Pages with Service-Layer Business Logic

## Status

Accepted

## Context

The application needs server-rendered UI with lightweight AJAX interactions and a maintainable place for business rules.

## Problem

Business logic can drift into transport handlers when there is no clear boundary.

## Decision

Use Razor Pages for request handling and place business logic in scoped services behind interfaces.

## Alternatives Considered

- Put more logic in page models
- Introduce a controller-based API layer

## Consequences

- Page handlers stay thin and testable.
- Services become the primary business-rule surface.
- Frontend contracts are defined by named handlers.

## Related Decisions

- `ADR-002-session-auth-and-utc-dates.md`

## Date

2026-08-22