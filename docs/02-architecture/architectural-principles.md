# Architectural Principles

- Separation of concerns MUST be preserved between handlers, services, data configuration, and client scripts.
- Dependency inversion SHOULD be used through interfaces for services.
- Cohesion SHOULD remain high within service classes and page files.
- Coupling SHOULD remain localized; avoid cross-layer shortcuts.
- Extensibility SHOULD come from stable seams, not speculative abstractions.
- Maintainability MUST outweigh premature generalization.
- Performance SHOULD be considered when query shape, reporting, or recurring-item copying is affected.