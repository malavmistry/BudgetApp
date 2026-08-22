# API Standards

## Authoritative scope

This file is the authoritative contract-rules document for BudgetApp page handlers.

## Current API style

- API-like behavior is implemented through named Razor Page handlers.
- AJAX handlers return `JsonResult`.
- UI handlers return `Page()` or `RedirectToPage()`.

## Rules

- Every handler MUST enforce session-based authentication when user-specific data is involved.
- Handler names MUST reflect purpose and HTTP shape.
- Business logic SHOULD be delegated to services.
- Request validation SHOULD happen before service invocation.