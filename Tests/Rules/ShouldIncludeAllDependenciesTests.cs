
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
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

        results.Single().Message.Should().Be("ServiceType 'Tests.TestParent' requires service 'Tests.ITestServiceA ChildA' but none are registered.");
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
    public void WhenAnyConstructorCanBeUsed_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParentWithTwoConstructors>();

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

    [TestMethod]
    public void WhenUsingGenericsLikeILogger_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton(typeof(ITestGeneric<>), typeof(TestGeneric<>));
        sc.AddSingleton<ITestService, GenericUser>();

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenDependencyIsIServiceProvider_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, ServiceProviderUser>();

        var results = new Validator()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Should().BeEmpty("IServiceProvider is available by default even if not registered");
    }

    [TestMethod]
    public void WhenDependencyIsIServiceProvider_WhenConfigured_ReturnsMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, ServiceProviderUser>();

        var results = new Validator()
            .With(new ShouldIncludeAllDependencies(c =>
            {
                c.AssumeIServiceProviderIsAvailable = false;
            }))
            .Validate(sc);

        results.Single().Message.Should().Be("ServiceType 'Tests.ServiceProviderUser' requires service 'System.IServiceProvider example' but none are registered.");
    }
}