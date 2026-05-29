using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Hackney.Core.Tests.JWT;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTokenFactoryTestNullServicesThrows()
    {
        IServiceCollection services = null;
        Action act = () => Hackney.Core.JWT.ServiceCollectionExtensions.AddTokenFactory(services);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddTokenFactoryTestAddsRequiredTypes()
    {
        var services = new ServiceCollection();
        services.AddTokenFactory();
        services.IsServiceRegistered<ITokenFactory, TokenFactory>().Should().BeTrue();
    }

    [Fact]
    public void AddTokenFactory_RegistersItsLogger()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddTokenFactory();
        var provider = services.BuildServiceProvider();
        var factory = provider.GetService<ITokenFactory>();
        var logger = provider.GetService<ILogger<TokenFactory>>();

        // assert
        factory.Should().NotBeNull();
        logger.Should().NotBeNull();
    }
}
