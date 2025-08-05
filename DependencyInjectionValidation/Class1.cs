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
                    Message = $"ImplementationType \"{g.Key.ImplementationType!.FullName}\" is registered for ServiceType \"{g.Key.ServiceType.FullName}\" {g.Count()} times."
                });
        }
    }

    public class Validators
    {
        public Validator Empty => new Validator();
    }

    public class Validator : IRule
    {
        public static Validators Predefined { get; } = new Validators();

        public List<IRule> Rules { get; private init;  } = new List<IRule>();

        public Validator With(IRule rule) => new Validator { Rules = Rules.Append(rule).ToList() };

        public Validator With<T>() where T : IRule, new() => new Validator { Rules = Rules.Append(new T()).ToList() };

        public Validator WithOut(IRule rule) => new Validator { Rules = Rules.Where(r => r != rule).ToList() };

        public Validator WithOut<T>()
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
