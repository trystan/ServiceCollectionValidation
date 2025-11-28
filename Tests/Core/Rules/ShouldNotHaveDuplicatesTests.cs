
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
}