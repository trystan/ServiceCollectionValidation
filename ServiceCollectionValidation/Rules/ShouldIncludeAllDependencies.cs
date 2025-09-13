using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

/// <summary>
/// Validate that at least one constructor of each registered ServiceTypes can be used to construct it.
/// </summary>
/// <remarks>
/// This is included in the <c>Validator.Predefined.Default</c> validator.
/// </remarks>
public class ShouldIncludeAllDependencies: IRule
{
    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        return services.SelectMany(descriptor => CheckConstructors(services, descriptor));
    }

    private List<Result> CheckConstructors(IServiceCollection services, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationType == null) return new List<Result>();

        var totalResults = new List<Result>();

        foreach (var constructor in descriptor.ImplementationType.GetConstructors())
        {
            var results = new List<Result>();

            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.HasDefaultValue) continue;

                // TODO: works, but seems like it could be better
                if (parameter.ParameterType.Name == "IEnumerable`1") continue;

                var isFulfilled = services.Any(s => IsMatch(parameter.ParameterType, s.ServiceType));
                if (!isFulfilled)
                {
                    var name = parameter.ParameterType.FullName ?? parameter.ParameterType.Name;
                    results.Add(new Result
                    {
                        Message = $"ServiceType '{descriptor.ImplementationType.FullName}' requires service '{name} {parameter.Name}' but none are registered."
                    });
                }
            }

            if (results.Count == 0)
            {
                // We found a good constructor - no need to keep looking.
                return results;
            }

            totalResults.AddRange(results);
        }

        return totalResults;
    }

    private bool IsMatch(Type lookingFor, Type lookingAt)
    {
        if (lookingFor == lookingAt) return true;

        // Handle ILogger<> style services
        if (lookingFor.IsGenericType && lookingAt.IsGenericType && lookingFor.GetGenericTypeDefinition() == lookingAt.GetGenericTypeDefinition()) return true;
        
        return false;
    }
}
