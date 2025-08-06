using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation.Rules;

/// <summary>
/// Validate that all ServiceTypes are registered in alphabetical order.
/// </summary>
/// <remarks>
/// This is not included in the <c>Validator.Predefined.Default</c> validator. It's mostly just an example.
/// </remarks>
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
