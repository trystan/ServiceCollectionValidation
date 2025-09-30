using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

/// <summary>
/// Validate that no <c>ServiceType</c> and <c>ImplementationType</c> combo are registered more than once.
/// </summary>
/// <remarks>
/// This is included in the <c>Validators.Predefined.Default</c> validator.
/// </remarks>
public record struct ShouldNotHaveDuplicates : IRule
{
    public IEnumerable<Result> Validate(IServiceCollection services)
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
