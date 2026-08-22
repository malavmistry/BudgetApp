# Agent Rules

## General guardrails

1. Do not invent project requirements.
2. Do not invent architecture when documentation exists.
3. Do not modify unrelated code.
4. Do not silently violate documented standards.
5. Do not overwrite architecture decisions without updating or creating an ADR when required.
6. Do not duplicate documentation unnecessarily.
7. Do not create new abstractions without justification.
8. Prefer existing project patterns.
9. Preserve backward compatibility unless explicitly instructed otherwise.
10. Escalate when requirements or documents conflict materially.
11. Validate changes before declaring completion.
12. Leave the project in an equivalent or better state.
13. Keep changes focused and traceable.
14. Update documentation when behavior or architecture changes.

## Minimum context rules

- Every agent MUST read `../00-core/project-context.md`.
- Every agent MUST read its own agent file before acting.
- Agents SHOULD load only the additional domain documents relevant to the task.

## Conflict rules

- Follow the instruction priority defined in `../README.md`.
- If two documents conflict at the same authority level, escalate and update the docs after resolution.