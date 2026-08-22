# Project Goals

## Product goals

- Help a user track personal budgets with minimal friction.
- Support both monthly budgets and custom envelope-style budgets.
- Make transaction entry fast, especially through Quick Add.
- Preserve accurate reporting across linked budgets and recurring items.
- Support export and review of budget history.

## Engineering goals

- Keep business rules in services.
- Keep request handling thin and predictable.
- Preserve data ownership boundaries through session-based user scoping.
- Maintain deterministic date handling across user timezones.
- Keep the codebase understandable for both humans and AI agents.
- Prefer incremental change over large rewrites.

## Non-goals

- Replacing the current Razor Pages architecture with a different web stack without an explicit decision record
- Introducing ASP.NET Identity without an explicit task and architecture decision
- Creating speculative abstractions that are not justified by current project needs