# Contracts

- View models define most request and response contracts.
- Frontend scripts MUST stay aligned with handler JSON property naming and shapes.
- `BudgetApi?handler=Detail` returns a camel-cased JSON payload.
- Quick Add saves through the same item-save contract used by the budget screen after resolving a primary budget.