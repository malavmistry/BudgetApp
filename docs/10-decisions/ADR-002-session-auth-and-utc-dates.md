# ADR-002: Session-Based Authentication and UTC Transaction Dates

## Status

Accepted

## Context

The application stores user-owned budget data and accepts transaction dates from browsers operating in different timezones.

## Problem

Incorrect ownership or timezone handling can produce unauthorized access or incorrect budget placement and reporting.

## Decision

- Use session-based authentication.
- Derive the current user from session state at the handler boundary.
- Store transaction dates in UTC.
- Convert between local time and UTC using the `userTimeZone` cookie.

## Alternatives Considered

- Trust client-supplied user ids
- Store local times directly

## Consequences

- Ownership checks remain centralized.
- Timezone conversion is mandatory for affected flows.
- Quick Add and report filtering depend on correct UTC conversion.

## Related Decisions

- `ADR-001-razor-pages-service-layer.md`

## Date

2026-08-22