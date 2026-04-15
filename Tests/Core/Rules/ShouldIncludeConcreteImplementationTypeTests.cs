using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
using System.Linq;

namespace Tests.Core.Rules;

[TestClass]
public class ShouldIncludeConcreteImplementationTypeTests
{
    [TestMethod]
    public void WhenFoundTypeIsNotRegistered_ReturnsMessages()
    {
        var sc = new ServiceCollection();

        var results = new Validator()
            .With<ShouldIncludeConcreteImplementationTypes<ITestServiceB>>()
            .Validate(sc);

        results.Single().Message.Should().Be("Type 'Tests.Core.TestServiceB' implements 'Tests.Core.ITestServiceB' and should be registered.");
    }

    [TestMethod]
    public void WhenFoundTypeIsRegistered_ReturnsNoMessages()
    {
        var sc = new ServiceCollection()
            .AddTransient<ITestServiceB, TestServiceB>();

        var results = new Validator()
            .With<ShouldIncludeConcreteImplementationTypes<ITestServiceB>>()
            .Validate(sc);

        results.Should().BeEmpty();
    }
}
