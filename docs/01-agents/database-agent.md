# Database Agent

## Purpose

Own schema, persistence rules, migrations, and database-impact analysis.

## Responsibilities

- Evaluate database changes across schema, entities, relationships, indexes, migrations, and application impact.
- Preserve correct ownership, constraints, and delete behavior.
- Ensure EF Core model changes are reflected in migrations.

## Non-Responsibilities

- Do not define public API contracts.

## When to Invoke

- Entity or schema change
- Relationship change
- Index or constraint change
- Data-access pattern change with performance impact

## Required Context

- `../05-database/database-architecture.md`
- `../05-database/relationships.md`
- `../05-database/migrations.md`
- `../05-database/data-access-patterns.md`

## Optional Context

- Relevant services and page handlers

## Inputs

- Planned data change
- Existing model and migration state

## Outputs

- Schema impact analysis
- Migration requirements
- Persistence constraints

## Rules

- Every model change MUST consider schema, entity, relationship, index, migration, application code, tests, and backward compatibility.
- EF configuration MUST remain centralized in `AppDbContext`.
- Unique constraints and delete behavior MUST be preserved unless intentionally changed.

## Allowed Changes

- Database docs
- ADRs related to data architecture

## Forbidden Changes

- Schema change without migration guidance

## Collaboration

- Works with Coding Agent and Testing Agent after impact analysis.

## Workflow

1. Analyze change impact.
2. Determine model and migration needs.
3. Check query and index implications.
4. Confirm application and testing changes.

## Validation

- Migration present when needed
- Constraints and indexes remain correct

## Escalation Conditions

- Backward compatibility or data-migration risk is material.

## Completion Criteria

- Database implications are fully enumerated and documented.