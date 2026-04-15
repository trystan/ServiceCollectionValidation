
using AwesomeAssertions;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.Core.Rules;

[TestClass]
public class ShouldNotHaveDuplicatesTests
{
    [TestMethod]
    public void WhenThereAreDuplicateRegisteredServices_ReturnsMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, TestService>();
        sc.AddTransient<ITestService, TestService>();

        var results = new Validator()
            .With<ShouldNotHaveDuplicates>()
            .Validate(sc);

        results.Single().Message.Should().Be("ImplementationType 'Tests.Core.TestService' is registered for ServiceType 'Tests.Core.ITestService' 2 times.");
    }
    
    [TestMethod]
    public void WhenThereAreNoDuplicateRegisteredServices_ReturnsNoMessages()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, TestService>();

        var results = new Validator()
            .With<ShouldNotHaveDuplicates>()
            .Validate(sc);

        results.Should().BeEmpty();
    }
    
    [TestMethod]
    public void WhenDuplicatesAreRegistered_ServiceProviderSilentlyResolvesLastRegistration()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<ITestService, TestService>();
        sc.AddTransient<ITestService, TestService>();

        var sp = sc.BuildServiceProvider();

        // The ServiceProvider does not throw and duplicates are silently accepted.
        // GetRequiredService returns only the last registration; the duplicate is invisible at runtime.
        sp.GetRequiredService<ITestService>().Should().BeOfType<TestService>();
        sp.GetServices<ITestService>().Should().HaveCount(2);
    }
}