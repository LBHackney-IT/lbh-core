using AwesomeAssertions;
using Hackney.Core.Sns;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hackney.Core.Tests.Sns;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSnsGatewayTestNullServicesThrows()
    {
        IServiceCollection services = null;
        Action act = () => Hackney.Core.Sns.ServiceCollectionExtensions.AddSnsGatewayAndClient(services);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddSnsGatewayTestAddsRequiredTypes()
    {
        var services = new ServiceCollection();
        services.AddSnsGatewayAndClient();
        services.IsServiceRegistered<ISnsGateway, SnsGateway>().Should().BeTrue();
    }
}
