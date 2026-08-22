# Authorization

- Never trust a client-supplied user id.
- Always filter data operations by the logged-in user.
- Deletion, rename, save, and list operations MUST respect user ownership boundaries.