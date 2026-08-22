# Logging

- Use `ILogger<T>` through dependency injection.
- Use structured logging with named placeholders.
- Do not log secrets, session tokens, passwords, or sensitive raw payloads.
- Log business-significant actions such as budget creation, deletion, or failure conditions where existing patterns already do so.