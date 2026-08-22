# Security Standards

## Authoritative scope

This file is the authoritative security-rules document for the repository.

## Rules

- Authentication MUST remain session-based unless an accepted decision changes it.
- Authorization MUST be enforced through current-user scoping on all user-owned data.
- Secrets MUST NOT be committed to the repository.
- Sensitive values MUST NOT be logged.
- All client input MUST be validated before trust.
- Least privilege SHOULD be maintained across configuration and operational choices.