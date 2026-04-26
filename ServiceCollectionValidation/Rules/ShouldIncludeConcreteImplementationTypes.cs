using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace ServiceCollectionValidation.Rules;

public class ShouldIncludeConcreteImplementationTypes<T> : IRule
{
    private readonly TypeFilterOptions? options;

    public ShouldIncludeConcreteImplementationTypes()
    {
    }

    public ShouldIncludeConcreteImplementationTypes(TypeFilterOptions? options = null)
    {
        this.options = options;
    }

    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        var serviceType = typeof(T);

        foreach (var implementer in GetImplementers())
        {
            if (services.Any(s => s.ServiceType == serviceType && s.ImplementationType == implementer)) continue;

            yield return new Result
            {
                Message = $"Type '{implementer.FullName}' implements '{serviceType.FullName}' and should be registered."
            };
        }
    }

    public IEnumerable<Type> GetImplementers()
    {
        var targetType = typeof(T);

        var assemblies = this.options?.Assemblies ?? AppDomain.CurrentDomain.GetAssemblies();
        var typeFilter = this.options?.TypeFilter ?? ((_) => true);

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(typeFilter))
        {
            if (type.IsAbstract) continue;

            if (type.IsInterface) continue;

            if (targetType.IsAssignableFrom(type)) yield return type;
        }
    }
}