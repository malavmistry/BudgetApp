# Dependency Boundaries

- Page models MUST NOT embed substantial business logic that belongs in services.
- Services SHOULD NOT depend on page-model types except view models already used as contracts where existing patterns do so.
- Data configuration MUST remain inside `AppDbContext`.
- Models MUST remain free of UI concerns.
- Client code MUST NOT assume server implementation details beyond documented contracts.