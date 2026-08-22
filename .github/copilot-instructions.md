# BudgetApp – Copilot Instructions

## Documentation Entry Point

The authoritative project instructions now live under `docs/`.

## Required Reading Order

1. `docs/README.md`
2. `docs/99-meta/agent-rules.md`
3. `docs/00-core/project-context.md`
4. The relevant agent definition in `docs/01-agents/`
5. The relevant domain-specific documents for the task

## Single Source of Truth Rule

- Treat `docs/` as the canonical project knowledge base.
- Do not duplicate or evolve project rules in this file.
- Update the authoritative document inside `docs/` when project guidance changes.

## High-Value Project Invariants

- BudgetApp uses ASP.NET Core Razor Pages plus a service layer.
- Authentication is session-based.
- User identity must come from session state, never client input.
- Transaction dates are stored in UTC and converted using the `userTimeZone` cookie.
- Quick Add resolves the primary monthly budget from the transaction date and must not ask the user to choose it.
- Business logic belongs in services and AJAX behavior is implemented through named page handlers.

## Authoritative Docs Map

- Project context: `docs/00-core/project-context.md`
- Project goals: `docs/00-core/project-goals.md`
- Architecture: `docs/02-architecture/system-architecture.md`
- Project structure: `docs/03-project-structure/project-structure.md`
- Coding standards: `docs/04-coding-standards/coding-standards.md`
- Database rules: `docs/05-database/database-architecture.md`
- API rules: `docs/06-api/api-standards.md`
- Testing rules: `docs/07-testing/testing-strategy.md`
- Security rules: `docs/08-security/security-standards.md`
- Agent guardrails: `docs/99-meta/agent-rules.md`

## Update Rule

If implementation changes any stable project behavior, contract, architecture rule, or workflow expectation, update `docs/` as part of the same change.
