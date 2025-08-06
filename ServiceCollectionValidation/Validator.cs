using ServiceCollectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectionValidation;

/// <summary>
/// The result of a validation.
/// </summary>
public class Result
{
    required public string Message {  get; set; }
}

/// <summary>
/// A validation rule that can be applied to a <c>ServiceCollection</c>.
/// </summary>
public interface IRule
{
    IEnumerable<Result> Validate(ServiceCollection services);
}

/// <summary>
/// Some predefined validators.
/// </summary>
/// <remarks>
/// You can easily add to this collection with extension methods on <c>Validators</c>.
/// </remarks>
public class Validators
{
    public Validator Empty => new Validator();

    public Validator Default = new Validator()
        .With<ShouldNotBeEmpty>()
        .With<ShouldNotHaveDuplicates>()
        .With<ShouldNotCaptureScope>()
        .With<ShouldIncludeAllDependencies>();
}

/// <summary>
/// A Validator takes a <c>ServiceCollection</c> and applies a list of rules and returns any validation messages.
/// </summary>
/// <remarks>
/// In addition to modifying the list of Rules on the Validator itself, you can use the <c>With()</c> and
/// <c>Without()</c> methods to return an updated copy. Validators can themselves be comosed using <c>With()</c>
/// and <c>Without()</c>.
/// </remarks>
public class Validator
{
    public static Validators Predefined { get; } = new Validators();

    public List<IRule> Rules { get; private init;  } = new List<IRule>();

    public Validator With(Validator other) => With(other.Rules.ToArray());

    public Validator With(params IRule[] rules) => new Validator { Rules = Rules.Concat(rules).ToList() };

    public Validator With<T>() where T : IRule, new() => new Validator { Rules = Rules.Append(new T()).ToList() };

    public Validator Without(Validator other) => Without(other.Rules.ToArray());

    public Validator Without(params IRule[] rules) => new Validator { Rules = Rules.Where(r => !rules.Contains(r)).ToList() };

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
