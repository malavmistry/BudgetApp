# BudgetApp Agentic Documentation System

This `docs/` folder is the persistent operating system for AI-assisted development in BudgetApp. It is the shared knowledge base, rules engine, architectural memory, and collaboration contract for agents working in this repository.

## Why it exists

Agents working on this project must be able to determine what the project does, where a change belongs, which rules apply, which files may be changed, what validation is required, and what documentation must be updated without loading the entire repository into context.

## How agents should use this system

Agents MUST load documentation progressively:

1. Always-read context
2. Project context
3. Task-specific context
4. Agent-specific context
5. Relevant architecture or decision records
6. Relevant implementation guidance

Agents SHOULD read the minimum context that can safely govern the requested work.

## Documentation hierarchy

- `00-core/`: project identity, goals, shared terminology, and engineering principles.
- `01-agents/`: agent definitions, ownership, scope, and collaboration rules.
- `02-architecture/`: architectural rules, patterns, and decisions.
- `03-project-structure/`: solution structure, folder organization, naming, and boundaries.
- `04-coding-standards/`: implementation rules.
- `05-database/`: schema, entities, relationships, migrations, and data-access rules.
- `06-api/`: endpoint, handler, contract, validation, and authentication guidance.
- `07-testing/`: testing strategy and expectations.
- `08-security/`: security rules and review focus.
- `09-development/`: workflows for feature work, bug fixes, refactoring, and Git.
- `10-decisions/`: ADR templates and accepted decisions.
- `11-tasks/`: active and completed task records.
- `12-checklists/`: executable review and delivery checklists.
- `99-meta/`: rules governing this documentation system itself.
- `technology/`: technology-specific rules that refine, but do not override, authoritative project rules unless explicitly stated.

## Authoritative documents

The following files are the primary sources of truth for their domains:

- `00-core/project-context.md`
- `00-core/project-goals.md`
- `02-architecture/system-architecture.md`
- `03-project-structure/project-structure.md`
- `04-coding-standards/coding-standards.md`
- `05-database/database-architecture.md`
- `06-api/api-standards.md`
- `07-testing/testing-strategy.md`
- `08-security/security-standards.md`
- `99-meta/agent-rules.md`
- `99-meta/change-management.md`

If a related rule appears elsewhere, the authoritative file wins.

## How agents determine what to read

1. Read `99-meta/agent-rules.md` and `00-core/project-context.md`.
2. Read the invoking agent document in `01-agents/`.
3. Read `00-core/project-goals.md` if the task changes user-visible behavior.
4. Read the relevant domain section for the requested change.
5. Read ADRs only when the change touches architecture, patterns, or long-lived decisions.
6. Read task documents under `11-tasks/active/` when the work is part of an ongoing tracked effort.

## Priority of instructions

1. Explicit task requirements
2. Security constraints
3. Accepted ADRs and architecture rules
4. Project-specific standards and workflow rules
5. Agent-specific instructions
6. Technology-specific guidance
7. General coding conventions
8. Agent assumptions

When instructions conflict in a material way, agents MUST follow `99-meta/change-management.md` and `99-meta/agent-rules.md` instead of silently choosing an interpretation.

## Documentation changes

Documentation changes are engineering changes. Agents MUST:

1. Update the authoritative document first.
2. Update dependent agent or checklist documents second.
3. Review for duplicated or conflicting guidance.
4. Record long-lived architectural decisions in `10-decisions/` when required.

## Architectural decisions

Use ADRs under `10-decisions/` for decisions with lasting consequences to boundaries, patterns, infrastructure, data shape, deployment, or public contracts. Do not create ADRs for local implementation details.

## Conflict resolution

Agents MUST detect conflicting rules, identify the authoritative source, escalate architectural conflicts, and update the documentation set after the conflict is resolved. See `99-meta/change-management.md`.

## Recommended entry path for new agents

1. `99-meta/agent-rules.md`
2. `00-core/project-context.md`
3. `03-project-structure/project-structure.md`
4. The relevant agent file in `01-agents/`
5. The relevant domain section