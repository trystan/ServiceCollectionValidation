using ServiceCollectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ServiceCollectionValidation;

public enum Severity
{
    Information,
    Warning,
    Error
}

/// <summary>
/// The result of a validation.
/// </summary>
public class Result
{
    public Severity Severity { get; init; } = Severity.Error;
    required public string Message {  get; init; }
}

/// <summary>
/// A validation rule that can be applied to a <c>IServiceCollection</c>.
/// </summary>
/// <remarks>
/// If the rule also implements <c>IRunBeforeValidation</c>, then it will be run before validation happens.
/// </remarks>
public interface IRule
{
    IEnumerable<Result> Validate(IServiceCollection services);
}

/// <summary>
/// Something that should happen before validation begins. It takes a copy of the original service collection that you can add to and the validator.
/// </summary>
public interface IRunBeforeValidation
{
    void RunBeforeValidation(IServiceCollection services);
    void RunBeforeValidation(IServiceCollection services, Validator validator) => RunBeforeValidation(services);
}

/// <summary>
/// Some predefined validators.
/// </summary>
/// <remarks>
/// You can easily add to this collection with extension methods on <c>Validators</c>.
/// </remarks>
public class Validators
{
    public static Validators Predefined { get; } = new Validators();

    public static Validator Empty => new Validator();

    public Validator Default = new Validator()
        .With<ShouldNotBeEmpty>()
        .With<ShouldNotHaveDuplicates>()
        .With<ShouldNotCaptureScope>()
        .With<ShouldIncludeAllDependencies>();
}

/// <summary>
/// A Validator takes a <c>IServiceCollection</c> and applies a list of rules and returns any validation messages.
/// </summary>
/// <remarks>
/// In addition to modifying the list of Rules on the validator itself, you can use the <c>With()</c> and
/// <c>Without()</c> methods to return an updated copy. Validators can themselves be composed using <c>With()</c>
/// and <c>Without()</c>.
/// </remarks>
public class Validator
{
    public List<IRunBeforeValidation> BeforeValidation { get; private init; } = [];

    public List<IRule> Rules { get; private init; } = [];

    public Validator() { }

    public Validator(IEnumerable<IRule> rules, IEnumerable<IRunBeforeValidation> beforeValidation)
    {
        Rules.AddRange(rules);
        BeforeValidation.AddRange(beforeValidation);
    }

    public Validator With(params IRule[] rules) => new Validator(this.Rules.Concat(rules), [.. this.BeforeValidation]);

    public Validator With(params IRunBeforeValidation[] beforeValidation) => new Validator([.. this.Rules], this.BeforeValidation.Concat(beforeValidation));

    public Validator With(Validator other) => this
        .With([.. other.Rules.Where(r => !Rules.Contains(r))])
        .With([.. other.BeforeValidation.Where(r => !BeforeValidation.Contains(r))]);

    public Validator With<T>()
        where T : IRule, new() => With(new T());

    public Validator WithBeforeValidation<T>()
        where T : IRunBeforeValidation, new() => With(new T());

    public Validator Without(params IRule[] rules) => new Validator(this.Rules.Where(r => !rules.Contains(r)), [.. this.BeforeValidation]);

    public Validator Without(params IRunBeforeValidation[] beforeValidation) => new Validator([.. this.Rules], this.BeforeValidation.Where(bv => beforeValidation.Contains(bv)));

    public Validator Without(Validator other) => this
        .Without([.. other.Rules])
        .Without([.. other.BeforeValidation]);

    public Validator Without<T>()
        where T : IRule, new() => new Validator(this.Rules.Where(r => r.GetType() != typeof(T)), [.. this.BeforeValidation]);

    public Validator WithoutBeforeValidation<T>()
        where T : IRunBeforeValidation, new() => new Validator([.. this.Rules], this.BeforeValidation.Where(r => r.GetType() != typeof(T)));

    public IEnumerable<Result> Validate(IServiceCollection services)
    {
        // Create a new collection so nothing can modify the original collection.
        // NOTE: the service descriptors themselves can still be modified, but
        // you should know you probably should not do that.
        var newServiceCollection = new ServiceCollection();

        foreach (var service in services)
        {
            newServiceCollection.Add(service);
        }

        foreach (var rule in BeforeValidation)
        {
            rule.RunBeforeValidation(newServiceCollection, this);
        }

        return [.. Rules.SelectMany(r => r.Validate(newServiceCollection))];
    }
}
