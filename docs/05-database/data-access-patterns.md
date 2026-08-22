# Data Access Patterns

- Use EF Core through services.
- Prefer eager loading when a handler requires shaped data for a view model.
- Keep query filters aligned with current user ownership.
- When linked-budget inclusion matters, ensure both primary-budget and additional-link paths are considered.
- When timezone-sensitive date filters are involved, define boundaries in local time and convert to UTC before querying.