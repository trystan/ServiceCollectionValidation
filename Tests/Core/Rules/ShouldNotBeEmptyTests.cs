
using AwesomeAssertions;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.Core.Rules;

[TestClass]
public class ShouldNotBeEmptyTests
{
    [TestMethod]
    public void WhenThereAreNoRegisteredServices_ReturnsMessages()
    {
        var sc = new ServiceCollection();

        var results = new Validator()
            .With<ShouldNotBeEmpty>()
            .Validate(sc);

        results.Single().Message.Should().Be("ServiceCollection should not be empty.");
    }
    
    [TestMethod]
    public void WhenThereAreRegisteredServices_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, TestService>();

        var results = new Validator()
            .With<ShouldNotBeEmpty>()
            .Validate(sc);

        results.Should().BeEmpty();
    }
}