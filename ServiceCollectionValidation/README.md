# ServiceCollectionValidation

Exactly what it sounds like, validate your service collection.

You can also use [ServiceCollectionValidation.AspNetCore](https://www.nuget.org/packages/ServiceCollectionValidation.AspNetCore) to add more validation for AspNetCore projects.

## Validate in your Startup.cs

In Startup:

```csharp
var services = new ServiceCollection();

// hundreds of lines of registration

var validator = Validators.Predefined.Default;
var results = validator.Validate(services);
if (results.Any())
{
    foreach (var result in results)
    {
        Console.WriteLine(result.Message);
    }
    throw new InvalidOperationException("ServiceCollection is not set up correctly.");
}
```

## Validate in your tests

```csharp
public void ServiceCollectionShouldBeValid()
{
    IServiceCollection services = null!;

    Host
        .CreateDefaultBuilder()
        .ConfigureServices(config =>
        {
            services = config;
        })
        .Build();

  var validator = Validators.Predefined.Default;

  var results = validator.Validate(services);

  results.Should().BeEmpty();
}
```

## Write new rules

You can write your own rules. This one is already defined, but not used by default.

```csharp
public class ShouldBeInAlphabeticalOrder : IRule
{
    public IEnumerable<Result> Validate(ServiceCollection services)
    {
        var types = services.Select(s => s.ServiceType);
        var firstOutOfOrder = types.Zip(types.OrderBy(t => t.Name)).FirstOrDefault(pair => pair.First != pair.Second);

        return firstOutOfOrder == default
            ? Enumerable.Empty<Result>()
            : [new Result { Message = $"Services should be registered in alphabetical order but found '{firstOutOfOrder.First.Name}' instead of expected '{firstOutOfOrder.Second.Name}'." }];
    }
}
```

## Write new functionality to run before validation

You can write your own functionality to run before validation.
One use is to get all the AspNetCore controllers and add them to the service collection to verify them. This is part of the definition from `ServiceCollectionValidation.AspNetCore` package.

```csharp
public class ShouldValidateControllers : IRunBeforeValidation
{
    public void RunBeforeValidation(IServiceCollection services)
    {
        foreach (var controller in GetControllers())
        {
            services.TryAddTransient(controller);
        }
    }
    
    // ...
}
```

## Use them with a validator

You can new up your own validator.

```csharp
var validator = new Validator();
validator.Rules.Add(new ShouldBeInAlphabeticalOrder());

var results = validator.Validate(services);
```

Or you can create a validator by composing rules, before valitations, and validators using `With()` and `Without()`.

```csharp
var validator = Validators.Predefined.Default
  .WithBeforeValidation<CheckCustomServices>()
  .With<ShouldAlwaysImplementAnInterface>()
  .With(new ShouldBeConfiguredFor(CurrentEnvironment));
  .Without<ShouldBeInAlphabeticalOrder>()
  .With(new SuperAdvancedValidator(strict: true));

var results = validator.Validate(services);
```

## Add your own predefined validator

You may want to define what rules to use in one place and reuse them, possibly across projects or repositories.
Here's an example of what that might look like in a common business app.

```csharp
namespace MyCompany.Common;

public static class ValidatorsExtensions
{
    public static Validator MyCompanyValidator(this Validators predefs)
    {
        return predefs.Default
            .With<ShouldBeInAlphabeticalOrder>()
            .With<ShouldBuildAllServices>()
            .With<MyCompany.Common.ShouldFollowOurNamingConventions>()
            .With<MyCompany.Common.ShouldHaveFewerThan50Methods>()
            .With(new MyCompany.Infrastructure.CommonValidator(strict: false));
            // Note: we need this because of bug #1234
            .Without<ShouldNotHaveDuplicates>()
    }
}
```

Then use it in any projects that reference that extension method.

```csharp
Validators.Predefined.MyCompanyValidator().Validate(services);
```

The `Default` validator is defined like this

```csharp
public Validator Default = new Validator()
    .With<ShouldNotBeEmpty>()
    .With<ShouldNotHaveDuplicates>()
    .With<ShouldNotCaptureScope>()
    .With<ShouldIncludeAllDependencies>();
```

## Existing rules

### ShouldBeInAlphabeticalOrder

Validates types are registererd in alphabeticcal order. Just a silly example.

This is not included in the `Default` validator.

### ShouldBuildAllServices

Validates that all services can actually be built - by actually building them.

This is not included in the `Default` validator since who knows what side effects building everything might have.

### ShouldIncludeAllDependencies

Validates each service has at least one constructor that can be used to instantiate it.

This is included in the `Default` validator.

### ShouldNotBeEmpty

This is included in the `Default` validator.

### ShouldNotCaptureScope

Validates that no services with Scope lifetime are injected into services with the Singleton lifetime.

This is included in the `Default` validator.

### ShouldNotHaveDuplicates

Validates the exact same implementation and service pair aren't registered more than once.

This is included in the `Default` validator.
