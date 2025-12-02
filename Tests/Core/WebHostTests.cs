using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceCollectionValidation.Rules;

namespace Tests.Core;

[TestClass]
public class WebHostTests
{
    [TestMethod]
    public void WithDefaultBuilder_ReturnsNoMessages()
    {
        IServiceCollection sc = null!;

        Host
            .CreateDefaultBuilder()
            .ConfigureServices(config =>
            {
                sc = config;
            })
            .Build();

        var validator = ServiceCollectionValidation.Validators.Predefined.Default
                .With<ShouldBuildAllServices>();

        var results = validator.Validate(sc!);

        results.Should().BeEmpty();
    }
}
