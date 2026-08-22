# Relationships

- `User -> Budgets`: one-to-many, cascade delete
- `Budget -> BudgetItems`: one-to-many, cascade delete
- `BudgetItem -> BudgetItemLinks`: one-to-many, cascade delete
- `BudgetItemLink -> LinkedBudget`: many-to-one, restrict delete
- `BudgetItem -> ItemName`: many-to-one, restrict delete
- `BudgetItem -> Category`: many-to-one, restrict delete
- `BudgetItem -> RecurringItem`: many-to-one, set null on delete
- `RecurringItem -> User`: many-to-one, cascade delete
- `RecurringItem -> ItemName`: many-to-one, restrict delete
- `RecurringItem -> Category`: many-to-one, restrict delete