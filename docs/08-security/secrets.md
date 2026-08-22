# Secrets

- Database passwords and other secrets MUST come from environment variables or secure configuration.
- `.env`-style examples MAY exist, but real secrets MUST NOT be committed.
- Agents MUST avoid echoing secrets in logs, docs, or review output.