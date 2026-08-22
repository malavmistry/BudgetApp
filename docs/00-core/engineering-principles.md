# Engineering Principles

## Core rules

- Prefer small, focused changes.
- Preserve established architecture unless the task explicitly changes it.
- Keep responsibilities separated across page models, services, data access, and client code.
- Prefer existing project patterns over new abstractions.
- Make rules explicit when repeated work reveals an undocumented convention.

## Decision heuristics

- If a change affects multiple layers, resolve ownership before implementation.
- If a rule exists in an authoritative document, apply it instead of inventing a new pattern.
- If a long-lived architectural tradeoff is introduced, record it through an ADR.
- If a change cannot be validated, do not claim completion.