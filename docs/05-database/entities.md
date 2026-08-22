# Entities

- `Budget`: owns budget metadata, user ownership, and time-bound or envelope identity.
- `BudgetItem`: owns primary transaction data and optional recurring-item link.
- `BudgetItemLink`: projects a primary item into additional budgets.
- `RecurringItem`: stores recurring transaction templates.
- `Category` and `ItemName`: lookup-style entities reused across items.