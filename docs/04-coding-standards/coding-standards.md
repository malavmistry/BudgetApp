# Coding Standards

## Authoritative scope

This file is the authoritative implementation-rules document for the repository.

## Core rules

- Code MUST follow existing local patterns before introducing new ones.
- Changes MUST be minimal and task-focused.
- Business rules MUST live in services unless the existing touched code clearly centralizes a narrow concern elsewhere.
- Session user identity MUST be read from `HttpContext.Session` using `SessionKeys`.
- Date parsing and formatting MUST use `AppConstants.DATE_FORMAT` where project workflows depend on that pattern.
- Currency handling MUST preserve existing truncation behavior where item persistence currently relies on it.

## Comments

- Add comments only when code intent is non-obvious.
- Do not add narration comments that merely restate the code.

## Nullability

- Respect nullable reference types.
- Prefer explicit null handling over null-forgiving operators unless the invariant is already enforced.

## Duplication

- Reuse existing helpers and patterns when duplication is obvious and safe to remove.
- Do not perform unrelated deduplication during a focused fix.