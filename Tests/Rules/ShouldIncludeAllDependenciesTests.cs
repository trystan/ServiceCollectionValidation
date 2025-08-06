
using AwesomeAssertions;
using DependencyInjectionValidation;
using DependencyInjectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.Rules;

[TestClass]
public class ShouldIncludeAllDependenciesTests
{
    [TestMethod]
    public void WhenDependencyIsNotRegistered_ReturnsMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParent>();

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Single().Message.Should().Be("ServiceType 'Tests.TestParent' requires service 'Tests.ITestServiceA' but none are registered.");
    }

    [TestMethod]
    public void WhenDependencyIsOptional_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParentWithOptionalChild>();

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenDependencyHasDefafult_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParentWithDefaultChild>();

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenDependencyIsRegistered_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParent>();
        sc.AddSingleton<ITestServiceA, TestServiceA>();

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenDependencyFactoryIsRegistered_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParent>();
        sc.AddSingleton<ITestServiceA>(sp => new TestServiceA());

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenImplementationFactoryIsRegistered_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService>(sp => new TestService());

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenImplementationInstanceIsRegistered_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService>(new TestService());

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty();
    }
}