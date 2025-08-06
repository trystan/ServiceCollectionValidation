using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

/// <summary>
/// Validate that at least something is added to the service collection.
/// </summary>
/// <remarks>
/// This is included in the <c>Validator.Predefined.Default</c> validator.
/// </remarks>
public class ShouldNotBeEmpty : IRule
{
    public IEnumerable<Result> Validate(ServiceCollection services)
    {
        return services.Any()
            ? Enumerable.Empty<Result>()
            : new List<Result> { new Result { Message = "ServiceCollection should not be empty." } };
    }
}
