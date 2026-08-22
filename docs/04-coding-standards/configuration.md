# Configuration

- Use configuration and environment variables for environment-specific values.
- Do not hardcode secrets.
- Preserve container-related checks such as `DOTNET_RUNNING_IN_CONTAINER` when touching startup flow.