# ServiceCollectionValidation.AspNetCore

Rules for ServiceCollectionValidation specificly for AspNetCore projects.

## New validator

A new predefined validator has been added. This includes the new rules for AspNetCore projects.

```csharp
Validators.Predefined.AspNetCore().Validate(services);
```

## New rules

### ShouldValidateControllers

This will scan all types in the current `AppDomain` that have `ControllerAttribute` but not `NonControllerAttribute` in their type heirarchy and validate them as though they were in the service collection.

This is included in the `AspNetCore` validator.
