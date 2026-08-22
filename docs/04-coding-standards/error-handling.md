# Error Handling

- Validate request preconditions at the page-handler boundary.
- Return appropriate `Unauthorized`, `BadRequest`, page redirects, or structured JSON errors according to handler type.
- Catch exceptions only when the code can translate them into a clearer boundary response or add meaningful logging.
- Do not swallow exceptions silently.