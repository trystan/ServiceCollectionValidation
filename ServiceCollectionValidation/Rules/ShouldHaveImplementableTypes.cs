using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

/// <summary>
/// Validate that every registered <c>ImplementationType</c> can actually be instantiated.
/// </summary>
/// <remarks>
/// Checks that the implementation type is not an interface, not abstract, not a generic type
/// definition, and has at least one public constructor. Registrations that use a factory or
/// a pre-built instance are skipped because they do not rely on the container constructing
/// the type directly.
/// This is included in the <c>Validators.Predefined.Default</c> validator.
/// </remarks>
public record struct ShouldHaveImplementableTypes : IRule
{
    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        var results = new List<Result>();

        foreach (var descriptor in services)
        {
            var type = descriptor.ImplementationType;

            // Factory and instance registrations are not constructed by the container.
            if (type == null) continue;

            if (type.IsInterface)
            {
                results.Add(new Result { Message = $"ImplementationType '{type.FullName}' registered for '{descriptor.ServiceType.FullName}' is an interface and cannot be instantiated." });
                continue;
            }

            if (type.IsAbstract)
            {
                results.Add(new Result { Message = $"ImplementationType '{type.FullName}' registered for '{descriptor.ServiceType.FullName}' is abstract and cannot be instantiated." });
                continue;
            }

            if (type.IsGenericTypeDefinition)
            {
                // Open generic registrations (e.g. typeof(ILogger<>)) are intentional so skip them.
                continue;
            }

            if (type.GetConstructors().Length == 0)
            {
                results.Add(new Result { Message = $"ImplementationType '{type.FullName}' registered for '{descriptor.ServiceType.FullName}' has no public constructors and cannot be instantiated." });
            }
        }

        return results;
    }
}