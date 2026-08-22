# ASP.NET Core Guidance

- Use Razor Pages handlers as the boundary for session checks and model validation.
- Return `JsonResult` for AJAX handlers.
- Redirect UI access to `/Login` when session-bound pages require auth.
- Keep page models thin and delegate business work to services.