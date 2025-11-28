# ServiceCollectionValidation.AspNetCore

Rules for ServiceCollectionValidation specificly for AspNetCore projects.

## New validator

A new predefined validator has been added. This includes the new rules for AspNetCore projects.

```csharp
Validators.Predefined.AspNetCore().Validate(services);
```

## New rules

### ShouldValidateControllers

Uses reflection to get all types in the current `AppDomain` that derive from `ControllerBase` and verify their dependencies are registered.

This is included in the `AspNetCore` validator.
