using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation.Rules;

/// <summary>
/// Validate that all parameters of all constructors of all registered ServiceTypes are registered.
/// </summary>
/// <remarks>
/// This is included in the <c>Validator.Predefined.Default</c> validator.
/// </remarks>
public class ShouldIncludeAllDependencies: IRule
{
    public IEnumerable<Result> Validate(ServiceCollection services)
    {
        var results = new List<Result>();

        foreach (var parent in services)
        {
            if (parent.ImplementationType == null) continue;

            foreach (var constructor in parent.ImplementationType.GetConstructors())
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    if (parameter.IsOptional) continue;

                    var child = services.FirstOrDefault(s => s.ServiceType == parameter.ParameterType);
                    if (child == null)
                    {
                        results.Add(new Result { Message = $"ServiceType '{parent.ImplementationType.FullName}' requires service '{parameter.ParameterType.FullName}' but none are registered." });
                    }
                }
            }
        }

        return results;
    }
}
