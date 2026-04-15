using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

public class ShouldIncludeConcreteImplementationTypes<T> : IRule
{
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

    public static IEnumerable<Type> GetImplementers()
    {
        var targetType = typeof(T);

        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        {
            if (type.IsAbstract) continue;

            if (type.IsInterface) continue;

            if (targetType.IsAssignableFrom(type)) yield return type;
        }
    }
}