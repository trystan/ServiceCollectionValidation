
using AwesomeAssertions;
using DependencyInjectionValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests;

[TestClass]
public class ShouldBeInAlphabeticalOrderTests
{
    [TestMethod]
    public void WhenRegisteredServicesAreNotInOrder_ReturnsMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestServiceA, ITestServiceA>();
        sc.AddTransient<ITestService, ITestService>();
        sc.AddTransient<ITestServiceB, ITestServiceB>();

        var results = new Validator()
            .With<ShouldBeInAlphabeticalOrder>()
            .Validate(sc);

        results.Single().Message.Should().Be("Services should be registered in alphabetical order but found 'ITestServiceA' instead of expected 'ITestService'.");
    }
    
    [TestMethod]
    public void WhenRegisteredServicesAreInOrder_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, ITestService>();
        sc.AddTransient<ITestServiceA, ITestServiceA>();
        sc.AddTransient<ITestServiceB, ITestServiceB>();

        var results = new Validator()
            .With<ShouldBeInAlphabeticalOrder>()
            .Validate(sc);

        results.Should().BeEmpty();
    }
}