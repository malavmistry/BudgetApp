# Schema

## Main tables

- `Users`
- `Categories`
- `ItemNames`
- `Budgets`
- `BudgetItems`
- `BudgetItemLinks`
- `RecurringItems`

## Special schema notes

- `BudgetItem.Amount` and `RecurringItem.Amount` use precision `decimal(18,2)`.
- `BudgetItem.TransactionDateUtc` stores UTC timestamps.
- `BudgetItemLink` enforces uniqueness on `(BudgetItemId, LinkedBudgetId)`.