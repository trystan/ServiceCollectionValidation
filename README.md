# ServiceCollectionValidation

Exactly what it sounds like, validate your service collection.

## Validate in your Setup.cs

In setup:

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
public void ValidateSetup()
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
  if (results.Any())
  {
      foreach (var result in results)
      {
          Console.WriteLine(result.Message);
      }
      Assert.Fail();
  }
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

## Use them with a validator

You can new up your own validator

```csharp
var validator = new Validator();
validator.Rules.Add(new ShouldBeInAlphabeticalOrder());

var results = validator.Validate(services);
```

or create one based on an existing one since `With()` and `Without()` create a copy without modifying the original.

```csharp
var validator = Validators.Predefined.Default
  .With<ShouldBeInAlphabeticalOrder>();

var results = validator.Validate(services);
```

or add an instance if you don't want to rely on a default constructor

```csharp
var validator = Validators.Predefined.Default
  .With(new ShouldBeConfiguredFor(CurrentEnvironment));

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

As shown in an earlier example, you can even compose validators themselves. They're "flattened" "so only the rules are added or removed.

```csharp
var validator = Validators.Predefined.MyCompanyValidator()
  .With(new SuperAdvancedValidator())
  .Without<ShouldBeInAlphabeticalOrder>();
```

## Existing rules

### ShouldBeInAlphabeticalOrder

This is not included in the `Default` validator.

### ShouldBuildAllServices

Validates that all services can actually be built.

This is not included in the `Default` validator.

### ShouldIncludeAllDependencies

Validates all dependencies are registered.

This is included in the `Default` validator.

### ShouldNotBeEmpty

This is included in the `Default` validator.

### ShouldNotCaptureScope

Validates that no services with Scope lifetime are injected into services with the Singleton lifetime.

This is included in the `Default` validator.

### ShouldNotHaveDuplicates

Validates the exact same implementation and service pair aren't registered more than once.

This is included in the `Default` validator.
