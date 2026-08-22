# Indexing

- Preserve the unique index on `Budgets(UserId, Name)`.
- Preserve the unique index on `BudgetItemLinks(BudgetItemId, LinkedBudgetId)`.
- Preserve unique lookup indexes such as `Users.Username`, `Categories.Name`, and `ItemNames.Name` unless requirements explicitly change.