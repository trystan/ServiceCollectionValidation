using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

public class ShouldIncludeAllDependenciesOptions : IEquatable<ShouldIncludeAllDependenciesOptions?>
{
    /// <summary>
    /// Assume that <c>IServiceProvider</c> is a valid dependency even if none is in the <c>ServiceCollection</c>.
    /// </summary>
    /// <remarks>
    /// <c>true</c> by default.
    /// </remarks>
    public bool AssumeIServiceProviderIsAvailable { get; set; } = true;

    public override bool Equals(object? obj)
    {
        return Equals(obj as ShouldIncludeAllDependenciesOptions);
    }

    public bool Equals(ShouldIncludeAllDependenciesOptions? other)
    {
        return other is not null &&
               AssumeIServiceProviderIsAvailable == other.AssumeIServiceProviderIsAvailable;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AssumeIServiceProviderIsAvailable);
    }

    public static bool operator ==(ShouldIncludeAllDependenciesOptions? left, ShouldIncludeAllDependenciesOptions? right)
    {
        return EqualityComparer<ShouldIncludeAllDependenciesOptions>.Default.Equals(left, right);
    }

    public static bool operator !=(ShouldIncludeAllDependenciesOptions? left, ShouldIncludeAllDependenciesOptions? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Validate that at least one constructor of each registered ServiceTypes can be used to construct it.
/// </summary>
/// <remarks>
/// This is included in the <c>Validators.Predefined.Default</c> validator.
/// </remarks>
public class ShouldIncludeAllDependencies(ShouldIncludeAllDependenciesOptions options) : IRule, IEquatable<ShouldIncludeAllDependencies?>
{
    private readonly ShouldIncludeAllDependenciesOptions _options = options;

    public ShouldIncludeAllDependencies()
        : this(new ShouldIncludeAllDependenciesOptions())
    {
    }

    public ShouldIncludeAllDependencies(Action<ShouldIncludeAllDependenciesOptions> configAction)
        : this()
    {
        configAction(this._options);
    }

    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        return services.SelectMany(descriptor => CheckConstructors(services, descriptor));
    }

    private List<Result> CheckConstructors(IServiceCollection services, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationType == null) return [];

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
                    if (parameter.ParameterType == typeof(IServiceProvider) && _options.AssumeIServiceProviderIsAvailable) continue;

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

    private static bool IsMatch(Type lookingFor, Type lookingAt)
    {
        if (lookingFor == lookingAt) return true;

        // Handle ILogger<> style services
        if (lookingFor.IsGenericType && lookingAt.IsGenericType && lookingFor.GetGenericTypeDefinition() == lookingAt.GetGenericTypeDefinition()) return true;
        
        return false;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ShouldIncludeAllDependencies);
    }

    public bool Equals(ShouldIncludeAllDependencies? other)
    {
        return other is not null &&
               _options.Equals(other._options);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_options);
    }

    public static bool operator ==(ShouldIncludeAllDependencies? left, ShouldIncludeAllDependencies? right)
    {
        return EqualityComparer<ShouldIncludeAllDependencies>.Default.Equals(left, right);
    }

    public static bool operator !=(ShouldIncludeAllDependencies? left, ShouldIncludeAllDependencies? right)
    {
        return !(left == right);
    }
}
