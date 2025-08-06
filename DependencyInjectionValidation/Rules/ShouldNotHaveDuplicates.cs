using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation.Rules;

/// <summary>
/// Validate that no <c>ServiceType</c> and <c>ImplementationType</c> combo are registered more than once.
/// </summary>
/// <remarks>
/// This is included in the <c>Validator.Predefined.Default</c> validator.
/// </remarks>
public class ShouldNotHaveDuplicates : IRule
{
    public IEnumerable<Result> Validate(ServiceCollection services)
    {
        return services
            .GroupBy(s => (s.ServiceType, s.ImplementationType))
            .Where(g => g.Count() > 1 && g.Key.ImplementationType != null)
            .Select(g => new Result
            {
                Message = $"ImplementationType '{g.Key.ImplementationType!.FullName}' is registered for ServiceType '{g.Key.ServiceType.FullName}' {g.Count()} times."
            });
    }
}
