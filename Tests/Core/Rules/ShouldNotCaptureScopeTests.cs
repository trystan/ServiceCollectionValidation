
using AwesomeAssertions;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;

namespace Tests.Core.Rules;

[TestClass]
public class ShouldNotCaptureScopeTests
{
    [TestMethod]
    public void WhenSingletonsDependOnScopedServices_ReturnsMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParent>();
        sc.AddScoped<ITestServiceA, TestServiceA>();

        var results = new Validator()
            .With<ShouldNotCaptureScope>()
            .Validate(sc);

        results.Single().Message.Should().Be("ServiceType 'Tests.Core.TestParent' with singleton lifetime captures service 'Tests.Core.ITestServiceA' with scoped lifetime.");
    }

    [TestMethod]
    public void WhenSingletonsDependOnSingletonServices_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParent>();
        sc.AddSingleton<ITestServiceA, TestServiceA>();

        var results = new Validator()
            .With<ShouldNotCaptureScope>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenSingletonsDependOnTransientServices_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParent>();
        sc.AddTransient<ITestServiceA, TestServiceA>();

        var results = new Validator()
            .With<ShouldNotCaptureScope>()
            .Validate(sc);

        results.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenSingletonCapturesScope_ServiceProviderThrowsWithScopeValidation()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestParent>();
        sc.AddScoped<ITestServiceA, TestServiceA>();

        // The ServiceProvider itself catches this when ValidateScopes is enabled
        var act = () => sc.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true })
            .GetRequiredService<ITestService>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot consume scoped service*");
    }
}