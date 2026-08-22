# Naming Conventions

- Interfaces for services MUST use the `I<Name>Service` pattern.
- Service implementations MUST use the `<Name>Service` pattern.
- Razor Page handlers MUST use `OnGet<Name>Async` or `OnPost<Name>Async` when asynchronous.
- View models SHOULD use descriptive `<Feature>ViewModel` naming.
- Constants SHOULD be upper snake case only when the existing type uses it; otherwise follow existing file conventions.
- JavaScript module functions SHOULD use descriptive camelCase names.