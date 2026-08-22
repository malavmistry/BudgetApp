# Authentication

- Login state is represented through session keys.
- Missing session state MUST block access to user-owned data.
- Session keys MUST be centralized under `SessionKeys`.