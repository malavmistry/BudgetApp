# Dependency Rules

- `Pages` MAY depend on `Services`, `ViewModels`, `Constants`, and framework abstractions.
- `Services` MAY depend on `Data`, `Models`, `ViewModels`, `Constants`, and framework abstractions.
- `Data` MAY depend on `Models` and EF Core.
- `Models` SHOULD remain persistence-focused and MUST NOT depend on `Pages` or `Services`.
- `wwwroot/js` depends on handler contracts, not on server internals.
- New dependencies across these boundaries require documented justification.