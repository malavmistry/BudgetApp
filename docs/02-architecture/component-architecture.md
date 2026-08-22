# Component Architecture

## Major components

- Budget management: budget list, detail, item save/delete, rename, monthly budget creation
- Category management: category CRUD and lookup
- Item name management: reusable item-name lookup and creation
- Recurring items: recurring template CRUD and activation
- Reporting: filtered report generation, pie charts, Excel export
- Authentication: session-backed login state

## Ownership map

- Budget and reporting behavior is service-led.
- Page handlers provide transport and auth boundaries.
- `budget.js` coordinates modal-heavy client flows for budgets, items, and Quick Add.

## Collaboration rules

- Components SHOULD communicate through services and handler contracts rather than directly crossing layers.