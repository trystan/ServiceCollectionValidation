using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

/// <summary>
/// Validate that all parameters of all constructors of all registered ServiceTypes are registered.
/// </summary>
/// <remarks>
/// This is included in the <c>Validator.Predefined.Default</c> validator.
/// </remarks>
public class ShouldIncludeAllDependencies: IRule
{
    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        var results = new List<Result>();

        foreach (var descriptor in services)
        {
            if (descriptor.ImplementationType == null) continue;

            foreach (var constructor in descriptor.ImplementationType.GetConstructors())
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    if (parameter.IsOptional) continue;

                    // TODO: works, but seems like it could be better
                    if (parameter.ParameterType.Name == "IEnumerable`1") continue;

                    var isFulfilled = services.Any(s => IsMatch(parameter.ParameterType, s.ServiceType));
                    if (!isFulfilled)
                    {
                        var name = parameter.ParameterType.FullName ?? parameter.ParameterType.Name;
                        results.Add(new Result {
                            Severity = Severity.Warning,
                            Message = $"ServiceType '{descriptor.ImplementationType.FullName}' requires service '{name} {parameter.Name}' but none are registered."
                        });
                    }
                }
            }
        }

        return results;
    }

    private bool IsMatch(Type lookingFor, Type lookingAt)
    {
        if (lookingFor == lookingAt) return true;
        if (lookingFor.IsGenericType && lookingAt.IsGenericType && lookingFor.GetGenericTypeDefinition() == lookingAt.GetGenericTypeDefinition()) return true;
        return false;
    }
}
