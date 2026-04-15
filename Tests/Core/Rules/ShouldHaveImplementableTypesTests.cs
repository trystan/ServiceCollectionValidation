using AwesomeAssertions;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests.Core.Rules;

[TestClass]
public class ShouldHaveImplementableTypesTests
{
    [TestMethod]
    public void WhenImplementationTypeIsAbstract_ReturnsMessage()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, AbstractTestService>();

        var results = new Validator()
            .With<ShouldHaveImplementableTypes>()
            .Validate(sc);

        results.Single().Message.Should().Be("ImplementationType 'Tests.Core.AbstractTestService' registered for 'Tests.Core.ITestService' is abstract and cannot be instantiated.");
    }

    [TestMethod]
    public void WhenImplementationTypeIsAnInterface_ReturnsMessage()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.Add(new ServiceDescriptor(typeof(ITestService), typeof(ITestServiceA), ServiceLifetime.Transient));

        var results = new Validator()
            .With<ShouldHaveImplementableTypes>()
            .Validate(sc);

        results.Single().Message.Should().Be("ImplementationType 'Tests.Core.ITestServiceA' registered for 'Tests.Core.ITestService' is an interface and cannot be instantiated.");
    }

    [TestMethod]
    public void WhenImplementationTypeHasNoPublicConstructors_ReturnsMessage()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, TestServiceWithPrivateConstructorOnly>();

        var results = new Validator()
            .With<ShouldHaveImplementableTypes>()
            .Validate(sc);

        results.Single().Message.Should().Be("ImplementationType 'Tests.Core.TestServiceWithPrivateConstructorOnly' registered for 'Tests.Core.ITestService' has no public constructors and cannot be instantiated.");
    }

    [TestMethod]
    public void WhenImplementationTypeIsConcreteWithPublicConstructor_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, TestService>();

        var results = new Validator()
            .With<ShouldHaveImplementableTypes>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenRegisteredWithFactory_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService>(sp => new TestService());

        var results = new Validator()
            .With<ShouldHaveImplementableTypes>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenRegisteredWithInstance_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService>(new TestService());

        var results = new Validator()
            .With<ShouldHaveImplementableTypes>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenRegisteredAsOpenGeneric_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient(typeof(ITestGeneric<>), typeof(TestGeneric<>));

        var results = new Validator()
            .With<ShouldHaveImplementableTypes>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenImplementationTypeIsAbstract_ServiceProviderThrowsOnBuild()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, AbstractTestService>();

        // The ServiceProvider rejects abstract types eagerly at BuildServiceProvider() time.
        var act = () => sc.BuildServiceProvider();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Cannot instantiate implementation type*");
    }

    [TestMethod]
    public void WhenImplementationTypeIsAnInterface_ServiceProviderThrowsOnBuild()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.Add(new ServiceDescriptor(typeof(ITestService), typeof(ITestServiceA), ServiceLifetime.Transient));

        // The ServiceProvider rejects interface types eagerly at BuildServiceProvider() time.
        var act = () => sc.BuildServiceProvider();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Cannot instantiate implementation type*");
    }

    [TestMethod]
    public void WhenImplementationTypeHasNoPublicConstructors_ServiceProviderThrowsOnResolution()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, TestServiceWithPrivateConstructorOnly>();

        var act = () =>
        {
            using var scope = sc.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<ITestService>();
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*suitable constructor*");
    }
}