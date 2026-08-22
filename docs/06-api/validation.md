# Validation

- Validate session before all user-bound handlers.
- Use model validation for body-bound models.
- Validate date strings with `AppConstants.DATE_FORMAT` where date input is involved.
- Reject invalid or conflicting inputs at the handler boundary before persistence.