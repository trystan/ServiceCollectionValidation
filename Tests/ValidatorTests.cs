
using AwesomeAssertions;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
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
    public void ValidatorsAreComposableToo()
    {
        var empty = new Validator();
        var first = new Validator()
            .With<ShouldBeInAlphabeticalOrder>();
        var second = new Validator()
            .With(first);
        var third = second
            .Without(first);

        second.Rules.Should().BeEquivalentTo(first.Rules);
        second.Rules.Should().NotBeSameAs(first.Rules);

        third.Rules.Should().BeEquivalentTo(empty.Rules);
        third.Rules.Should().NotBeSameAs(empty.Rules);
    }
}