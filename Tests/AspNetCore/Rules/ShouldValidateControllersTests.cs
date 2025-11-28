using AspNetCore.Rules;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceCollectionValidation;
using ServiceCollectionValidation.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Core;

namespace Tests.AspNetCore.Rules;

[TestClass]
public class ShouldValidateControllersTests
{
    [TestMethod]
    public void DetectsTypesWithControllerAttribute()
    {
        var controllers = ShouldValidateControllers.GetControllers();

        var expectedController = typeof(TestController);

        controllers.Should().Contain(expectedController);
    }

    [TestMethod]
    public void IgnoresTypesWithNonControllerAttribute()
    {
        var controllers = ShouldValidateControllers.GetControllers();

        var expectedController = typeof(TestNonController);

        controllers.Should().NotContain(expectedController);
    }

    [TestMethod]
    public void DoesNotModifyServiceCollection()
    {
        var sc = new ServiceCollection();

        new Validator()
            .With<ShouldValidateControllers>()
            .Validate(sc);

        sc.Should().BeEmpty();
    }

    [TestMethod]
    public void WhenControllerDependencyIsMissing_ReturnsMessage()
    {
        var sc = new ServiceCollection();

        var results = new Validator()
            .With<ShouldValidateControllers>()
            .With<ShouldIncludeAllDependencies>()
            .Validate(sc);

        results.Single().Message.Should().Be("ServiceType 'Tests.AspNetCore.TestController' requires service 'Tests.Core.ITestService service' but none are registered.");
    }
}
