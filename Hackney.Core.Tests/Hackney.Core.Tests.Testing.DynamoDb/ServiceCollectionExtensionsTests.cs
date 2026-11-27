using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AwesomeAssertions;
using Hackney.Core.Testing.DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.Testing.DynamoDb;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureDynamoDbFixture_GivenNullServices_ThrowsArgumentNullException()
    {
        // arrange
        IServiceCollection services = null;

        // act
        Action act = () => services.ConfigureDynamoDbFixture();

        // assert
        act.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "services");
    }

    [Fact]
    public void ConfigureDynamoDbFixture_GivenEmptyServiceCollection_RegistersSingletonFixture()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.ConfigureDynamoDbFixture();

        // assert
        var registration = services.Single(x => x.ServiceType == typeof(IDynamoDbFixture));
        registration.Lifetime.Should().Be(ServiceLifetime.Singleton);
        registration.ImplementationType.Should().Be(typeof(DynamoDbFixture));
    }

    [Fact]
    public void ConfigureDynamoDbFixture_GivenCalledTwice_RegistersFixtureOnlyOnce()
    {
        // arrange
        // TryAdd must keep a second call from duplicating the singleton (e.g. factory + Startup both wire it).
        var services = new ServiceCollection();

        // act
        services.ConfigureDynamoDbFixture();
        services.ConfigureDynamoDbFixture();

        // assert
        services.Count(x => x.ServiceType == typeof(IDynamoDbFixture)).Should().Be(1);
    }

    [Fact]
    public void ConfigureDynamoDbFixture_GivenServiceCollection_ReturnsSameInstance()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        var returned = services.ConfigureDynamoDbFixture();

        // assert
        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void ConfigureDynamoDbFixture_GivenClientAndContextRegistered_ResolvesUsableFixture()
    {
        // arrange
        // Mirrors consumer factories: ConfigureDynamoDB (client + context) then ConfigureDynamoDbFixture.
        var mockClient = new Mock<IAmazonDynamoDB>();
        var mockContext = new Mock<IDynamoDBContext>();
        var services = new ServiceCollection();

        services.AddSingleton(mockClient.Object);
        services.AddSingleton(mockContext.Object);
        services.ConfigureDynamoDbFixture();

        // act
        var fixture = services.BuildServiceProvider().GetRequiredService<IDynamoDbFixture>();

        // assert
        fixture.Should().BeOfType<DynamoDbFixture>();
        fixture.DynamoDb.Should().BeSameAs(mockClient.Object);
        fixture.DynamoDbContext.Should().BeSameAs(mockContext.Object);
    }
}
