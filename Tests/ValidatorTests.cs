
using AwesomeAssertions;
using DependencyInjectionValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class ValidatorTests
{
    [TestMethod]
    public void WhenAddingRules_PreservesTheOrigional()
    {
        var first = new Validator();
        var second = first
            .With<ShouldBeInAlphabeticalOrder>();

        first.Rules.Should().BeEmpty();
        second.Rules.Should().HaveCount(1);
    }

    [TestMethod]
    public void WhenRemovingRules_PreservesTheOrigional()
    {
        var first = new Validator()
            .With<ShouldBeInAlphabeticalOrder>();
        var second = first
            .Without<ShouldBeInAlphabeticalOrder>();

        first.Rules.Should().HaveCount(1);
        second.Rules.Should().BeEmpty();
    }

    [TestMethod]
    public void ValidatorsAreRulesToo()
    {
        var first = new Validator()
            .With<ShouldBeInAlphabeticalOrder>();
        var second = new Validator()
            .With(first);

        second.Rules.Should().BeEquivalentTo(first.Rules);
        second.Rules.Should().NotBeSameAs(first.Rules);
    }
}