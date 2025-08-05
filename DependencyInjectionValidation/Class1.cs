using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation
{
    public class Result
    {
        required public string Message {  get; set; }
    }

    public interface IRule
    {
        IEnumerable<Result> Validate(ServiceCollection services);
    }

    public class ShouldNotBeEmpty : IRule
    {
        public IEnumerable<Result> Validate(ServiceCollection services)
        {
            return services.Any()
                ? Enumerable.Empty<Result>()
                : new List<Result> { new Result { Message = "ServiceCollection should not be empty." } };
        }
    }

    public class ShouldNotHaveDuplicates : IRule
    {
        public IEnumerable<Result> Validate(ServiceCollection services)
        {
            return services
                .GroupBy(s => (s.ServiceType, s.ImplementationType))
                .Where(g => g.Count() > 1 && g.Key.ImplementationType != null)
                .Select(g => new Result
                {
                    Message = $"ImplementationType '{g.Key.ImplementationType!.FullName}' is registered for ServiceType '{g.Key.ServiceType.FullName}' {g.Count()} times."
                });
        }
    }

    public class ShouldNotCaptureScope: IRule
    {
        public IEnumerable<Result> Validate(ServiceCollection services)
        {
            var singletons = services.Where(s => s.Lifetime == ServiceLifetime.Singleton).ToList();
            var scopes = services.Where(s => s.Lifetime == ServiceLifetime.Scoped).ToList();
            var transients = services.Where(s => s.Lifetime == ServiceLifetime.Transient).ToList();

            var results = new List<Result>();

            foreach (var singleton in singletons)
            {
                if (singleton.ImplementationType == null) continue;

                foreach (var constructor in singleton.ImplementationType.GetConstructors())
                {
                    foreach (var parameter in constructor.GetParameters())
                    {
                        var capturedScope = scopes.FirstOrDefault(s => s.ServiceType == parameter.ParameterType);
                        if (capturedScope != null)
                        {
                            results.Add(new Result { Message = $"ServiceType '{singleton.ImplementationType.FullName}' with singleton lifetime captures service '{capturedScope.ServiceType.FullName}' with scoped lifetime." });
                        }
                    }
                }
            }

            return results;
        }
    }

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

    public class ShouldBuildAllServices : IRule
    {
        public IEnumerable<Result> Validate(ServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            var results = new List<Result>();

            foreach (var service in services)
            {
                try
                {
                    sp.GetService(service.ServiceType);
                }
                catch (Exception e)
                {
                    results.Add(new Result { Message = $"{e.GetType().FullName}: {e.Message}" });
                }
            }

            return results;
        }
    }

    public class ShouldBeInAlphabeticalOrder : IRule
    {
        public IEnumerable<Result> Validate(ServiceCollection services)
        {
            var types = services.Select(s => s.ServiceType);
            var firstOutOfOrder = types.Zip(types.OrderBy(t => t.Name)).FirstOrDefault(pair => pair.First != pair.Second);

            return firstOutOfOrder == default
                ? Enumerable.Empty<Result>()
                : [new Result { Message = $"Services should be registered in alphabetical order but found '{firstOutOfOrder.First.Name}' instead of expected '{firstOutOfOrder.Second.Name}'." }];
        }
    }

    public class Validators
    {
        public Validator Empty => new Validator();

        public Validator Default = new Validator()
            .With<ShouldNotBeEmpty>()
            .With<ShouldNotHaveDuplicates>()
            .With<ShouldNotCaptureScope>()
            .With<ShouldIncludeAllDependencies>();
    }

    public class Validator : IRule
    {
        public static Validators Predefined { get; } = new Validators();

        public List<IRule> Rules { get; private init;  } = new List<IRule>();

        public Validator With(IRule rule) => new Validator { Rules = Rules.Append(rule).ToList() };

        public Validator With<T>() where T : IRule, new() => new Validator { Rules = Rules.Append(new T()).ToList() };

        public Validator Without(IRule rule) => new Validator { Rules = Rules.Where(r => r != rule).ToList() };

        public Validator Without<T>()
            where T : IRule, new()
        {
            var t = new T().GetType();
            return new Validator { Rules = Rules.Where(r => r.GetType() != t).ToList() };
        }

        public IEnumerable<Result> Validate(ServiceCollection services)
        {
            return Rules.SelectMany(r => r.Validate(services));
        }
    }
}
