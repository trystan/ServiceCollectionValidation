using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceCollectionValidation;

namespace AspNetCore.Rules;

public class ShouldValidateControllers : IRule
{
    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        var totalResults = new List<Result>();

        var controllerBase = typeof(ControllerBase);

        var controllers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(controllerBase));

        foreach (var controller in controllers)
        {
            foreach (var constructor in controller.GetConstructors())
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
                            Message = $"Controller '{controller.FullName}' requires service '{name} {parameter.Name}' but none are registered."
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
