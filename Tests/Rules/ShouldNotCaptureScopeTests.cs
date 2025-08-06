
using AwesomeAssertions;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.Rules;

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

        results.Single().Message.Should().Be("ServiceType 'Tests.TestParent' with singleton lifetime captures service 'Tests.ITestServiceA' with scoped lifetime.");
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
}