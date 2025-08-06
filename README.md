# ServiceCollectionValidation

Exactly what it sounds like, validate your service collection.

## Validate in your Setup.cs

In setup:

```csharp
var services = new ServiceCollection();

// hundreds of lines of registration

var validator = Validator.Predefined.Default;
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
  var services = new ServiceCollection();

  new Setup()
    .AddServices(services);

  var validator = Validator.Predefined.Default;
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

or create one based on an existing one

```csharp
var validator = Validator.Predefined.Default
  .With<ShouldBeInAlphabeticalOrder>();

var results = validator.Validate(services);
```

## Create your own default validator

You may want to define what rules to use in one place and reuse them, possibly across projects. Here's an example of what that might look like.

```csharp
namespace MyCompany.Common;

public static class ValidatorsExtensions
{
  public static Validator MyCompanyValidator(this Validators validators)
  {
    return Validator.Predefined.Default
        .With<ShouldBeInAlphabeticalOrder>()
        .With<ShouldBuildAllServices>()
        .With<MyCompany.Common.ShouldFollowOurNamingConventions>()
        .With<MyCompany.Common.ShouldHaveFewerThan50Methods>()
        // Note: we need this because of bug #1234
        .Without<ShouldNotHaveDuplicates>();
  }
}
```

Then use it in any projects that references your project that has that extension method.

```csharp
Validator.Predefined.MyCompanyValidator().Validate(services);
```

The `Default` validator is defined like this

```csharp
public Validator Default = new Validator()
    .With<ShouldNotBeEmpty>()
    .With<ShouldNotHaveDuplicates>()
    .With<ShouldNotCaptureScope>()
    .With<ShouldIncludeAllDependencies>();
```

You can even compose validators themselves

```csharp
var validator = Validator.Predefined.MyCompanyValidator
  .With(new SuperAdvancedValidator())
  .Without<ShouldBeInAlphabeticalOrder>();
```

## Existing rules

### ShouldBeInAlphabeticalOrder

This is not included in the `Default` validator.

### ShouldBuildAllServices

This is not included in the `Default` validator.

### ShouldIncludeAllDependencies

This is included in the `Default` validator.

### ShouldNotBeEmpty

This is included in the `Default` validator.

### ShouldNotCaptureScope

This is included in the `Default` validator.

### ShouldNotHaveDuplicates

This is included in the `Default` validator.
