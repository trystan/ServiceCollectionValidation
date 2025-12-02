using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ServiceCollectionValidation.AspNetCore.Rules;

/// <summary>
/// This will scan all types in the current <c>AppDomain</c> that have <c>ControllerAttribute</c> but not <c>NonControllerAttribute</c> in their type heirarchy and validate them as though they were in the service collection.
/// </summary>
/// <remarks>
/// This is included in the <c>Validators.Predefined.AspNetCore()</c> validator.
/// </remarks>
public class ShouldValidateControllers : IRule, IRunBeforeValidation
{
    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        return [];
    }

    public void RunBeforeValidation(IServiceCollection services)
    {
        foreach (var controller in GetControllers())
        {
            services.TryAddTransient(controller);
        }
    }

    public static IEnumerable<Type> GetControllers()
    {
        var controllerAttribute = typeof(ControllerAttribute);
        var nonControllerAttribute = typeof(NonControllerAttribute);

        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        {
            if (type.IsAbstract) continue;

            if (type.IsInterface) continue;

            var doesImplementControllerAttribute = RecursiveAny(type, t => t.CustomAttributes.Any(a => a.AttributeType == controllerAttribute));
            if (!doesImplementControllerAttribute) continue;

            var shouldIgnore = RecursiveAny(type, t => t.CustomAttributes.Any(a => a.AttributeType == nonControllerAttribute)); ;
            if (shouldIgnore) continue;
            
            yield return type;
        }
    }

    private static bool RecursiveAny(Type type, Func<Type, bool> predicate)
    {
        if (predicate(type)) return true;
        if (type.BaseType != null && RecursiveAny(type.BaseType, predicate)) return true;
        return type.GetInterfaces().Any(i => RecursiveAny(i, predicate));
    }
}
