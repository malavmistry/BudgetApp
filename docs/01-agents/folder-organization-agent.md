# Folder Organization Agent

## Purpose

Own file placement and grouping rules within the chosen layer.

## Responsibilities

- Determine where new files belong.
- Maintain predictable naming and grouping.
- Prevent dumping-ground folders.

## Non-Responsibilities

- Do not decide architecture or layer ownership.

## When to Invoke

- New file creation
- Reorganization within an existing layer

## Required Context

- `../03-project-structure/folder-organization.md`
- `../03-project-structure/naming-conventions.md`

## Optional Context

- Existing neighboring files

## Inputs

- Planned file additions or moves

## Outputs

- Folder placement decision
- Naming guidance

## Rules

- Prefer existing folder patterns.
- Avoid folders named `misc`, `helpers`, `common`, or `utils` unless an ADR explicitly allows them.
- Keep page-specific JavaScript in the matching `wwwroot/js` file or a closely related file.

## Allowed Changes

- Structure docs

## Forbidden Changes

- Structure changes that conflict with layer ownership

## Collaboration

- Works after Project Structure Agent when layer ownership is unclear.

## Workflow

1. Find the owning layer.
2. Inspect neighboring file patterns.
3. Place files where future agents will expect them.

## Validation

- Placement aligns with existing conventions.

## Escalation Conditions

- Existing structure is inconsistent enough to require a documented reorganization.

## Completion Criteria

- File placement is predictable and documented if new patterns are introduced.