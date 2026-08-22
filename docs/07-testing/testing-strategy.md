# Testing Strategy

## Authoritative scope

This file is the authoritative testing guidance for the repository.

## Current state

- The repository does not currently include a dedicated automated test project.
- Focused build validation and behavior-scoped manual verification are therefore part of the present workflow.

## Strategy

- Changes SHOULD be validated with the narrowest executable check available.
- Behavior changes SHOULD include regression-oriented validation steps.
- When a future test project is introduced, tests SHOULD focus on service behavior and critical handler contracts.