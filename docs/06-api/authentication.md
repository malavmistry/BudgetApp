# Authentication

- Authentication is session-based.
- The current user id MUST be read from `HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID)`.
- API handlers return `Unauthorized()` when session state is missing.
- Page handlers redirect to `/Login` when the UI requires session state.