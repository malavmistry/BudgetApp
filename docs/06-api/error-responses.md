# Error Responses

- Use `Unauthorized()` for missing session auth.
- Use `BadRequest` for invalid request shapes or invalid date input.
- Use structured JSON error payloads when the frontend expects AJAX error reporting.
- Translate business validation exceptions into user-meaningful messages when current patterns already do so.