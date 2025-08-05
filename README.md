# DependencyInjectionValidation

Exactly what it sounds like, validate your service collection.

## Validate in your Setup.cs

In setup:

```csharp
var services = new ServiceCollection();

// hundreds of lines of registration

var results = DependencyInjectionValidation.Validators.Default.Validate(services);
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
  var services = new Setup().ServiceCollection;

  var results = DependencyInjectionValidation.Validators.Default.Validate(services);
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

```csharp
public class ShouldBeInAlphabeticalOrder : IRule
{
    public IEnumerable<Result> Validate(ServiceCollection services)
    {
        return services.Any()
            ? Enumerable.Empty<Result>()
            : new List<Result> { new Result { Message = "ServiceCollection should not be empty." } };
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

or add to an existing one

```csharp
var validator = DependencyInjectionValidation.Validators.Default
  .With<ShouldBeInAlphabeticalOrder>();

var results = validator.Validate(services);
```

## Create your own default ruleset

You may want to reuse the same validation rules across projects. Easy.

```csharp
namespace MyCompany.Common;

public static class ValidatorsExtensions
{
  public static Validator MyCompanyValidator(this Validators validators) => validators.Default.With<ShouldBeInAlphabeticalOrder>();
}
```

Then use it in any projects that reference your project that has `MyCompany.Common`.

```csharp
DependencyInjectionValidation.Validators.MyCompanyValidator().Validate(services);
```