using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation.Rules;

/// <summary>
/// Validate that there are no Singleton services that depend on Scoped services.
/// </summary>
/// <remarks>
/// This is included in the <c>Validator.Predefined.Default</c> validator.
/// </remarks>
public class ShouldNotCaptureScope: IRule
{
    public IEnumerable<Result> Validate(ServiceCollection services)
    {
        var singletons = services.Where(s => s.Lifetime == ServiceLifetime.Singleton).ToList();
        var scopes = services.Where(s => s.Lifetime == ServiceLifetime.Scoped).ToList();
        var transients = services.Where(s => s.Lifetime == ServiceLifetime.Transient).ToList();

        var results = new List<Result>();

        foreach (var singleton in singletons)
        {
            if (singleton.ImplementationType == null) continue;

            foreach (var constructor in singleton.ImplementationType.GetConstructors())
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    var capturedScope = scopes.FirstOrDefault(s => s.ServiceType == parameter.ParameterType);
                    if (capturedScope != null)
                    {
                        results.Add(new Result { Message = $"ServiceType '{singleton.ImplementationType.FullName}' with singleton lifetime captures service '{capturedScope.ServiceType.FullName}' with scoped lifetime." });
                    }
                }
            }
        }

        return results;
    }
}
