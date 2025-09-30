using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation.Rules;

/// <summary>
/// Create a ServiceProvider and validate that all ServiceTypes can actually be created.
/// </summary>
/// <remarks>
/// This is not included in the <c>Validators.Predefined.Default</c> validator. It may be a good idea, but it may have unwanted side effects.
/// </remarks>
public record struct ShouldBuildAllServices : IRule
{
    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var results = new List<Result>();

        foreach (var service in services)
        {
            // TODO
            if (service.ServiceType.ContainsGenericParameters) continue;

            try
            {
                scope.ServiceProvider.GetRequiredService(service.ServiceType);
            }
            catch (Exception e)
            {
                results.Add(new Result { Message = $"{e.GetType().FullName}: {e.Message}" });
            }
        }

        return results;
    }
}
