using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AwesomeAssertions;
using Hackney.Core.DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb;

public class DynamoDbInitilisationExtensionsTests
{
    [Fact]
    public void ConfigureDynamoDB_NullServices_ThrowsArgumentNullException()
    {
        // arrange
        IServiceCollection services = null;

        // act
        Action act = () => DynamoDbInitilisationExtensions.ConfigureDynamoDB(services);

        // assert
        act.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "services");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("false")]
    [InlineData("true")]
    public void ConfigureDynamoDB_RegistersClientAndContext_ForEachLocalModeValue(string localModeEnvVar)
    {
        // arrange
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", localModeEnvVar);
        var services = new ServiceCollection();

        // act
        services.ConfigureDynamoDB();

        // assert
        services.Any(x => x.ServiceType == typeof(IAmazonDynamoDB)).Should().BeTrue();
        var clientRegistration = services.First(x => x.ServiceType == typeof(IAmazonDynamoDB));
        clientRegistration.Lifetime.Should().Be(ServiceLifetime.Singleton);
        clientRegistration.ImplementationFactory.Should().NotBeNull();

        services.Any(x => x.ServiceType == typeof(IDynamoDBContext)).Should().BeTrue();
        var contextRegistration = services.First(x => x.ServiceType == typeof(IDynamoDBContext));
        contextRegistration.Lifetime.Should().Be(ServiceLifetime.Scoped);
        contextRegistration.ImplementationFactory.Should().NotBeNull();

        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", null);
    }

    [Fact]
    public void ConfigureDynamoDB_LocalMode_RegistersClientWithServiceUrlAndContext()
    {
        // arrange
        var url = "http://localhost:8000/";
        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", url);
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", "true");
        var services = new ServiceCollection();

        // act
        services.ConfigureDynamoDB();
        var serviceProvider = services.BuildServiceProvider();

        // assert
        var amazonDynamoDB = serviceProvider.GetService<IAmazonDynamoDB>();
        amazonDynamoDB.Should().NotBeNull();
        amazonDynamoDB.Config.ServiceURL.Should().Be(url);

        var dynamoDBContext = serviceProvider.GetService<IDynamoDBContext>();
        dynamoDBContext.Should().NotBeNull();

        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", null);
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", null);
    }

    [Fact]
    public void ConfigureDynamoDB_LeavesExistingClientRegistration()
    {
        // arrange
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", "true");
        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", "http://localhost:8000/");
        var existingClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = "http://existing:8000/" });
        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB>(existingClient);

        // act
        services.ConfigureDynamoDB();
        var serviceProvider = services.BuildServiceProvider();

        // assert
        services.Count(x => x.ServiceType == typeof(IAmazonDynamoDB)).Should().Be(1);
        serviceProvider.GetService<IAmazonDynamoDB>().Should().BeSameAs(existingClient);

        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", null);
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", null);
    }

    [Fact]
    public void ConfigureDynamoDB_LeavesExistingContextRegistration()
    {
        // arrange
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", "true");
        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", "http://localhost:8000/");
        var existingContext = new Mock<IDynamoDBContext>().Object;
        var services = new ServiceCollection();
        services.AddScoped(_ => existingContext);

        // act
        services.ConfigureDynamoDB();
        var serviceProvider = services.BuildServiceProvider();

        // assert
        services.Count(x => x.ServiceType == typeof(IDynamoDBContext)).Should().Be(1);
        serviceProvider.GetService<IDynamoDBContext>().Should().BeSameAs(existingContext);

        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", null);
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", null);
    }

    [Fact]
    public void ConfigureDynamoDB_BuiltContext_KeepsTableMetadataFetchingEnabled()
    {
        // arrange
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", "true");
        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", "http://localhost:8000/");
        var services = new ServiceCollection();
        services.ConfigureDynamoDB();
        var serviceProvider = services.BuildServiceProvider();

        // act
        var context = serviceProvider.GetService<IDynamoDBContext>();
        var config = (DynamoDBContextConfig)typeof(DynamoDBContext)
            .GetProperty("Config", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(context);

        // assert
        config.DisableFetchingTableMetadata.Should().BeFalse();

        Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", null);
        Environment.SetEnvironmentVariable("DynamoDb_LocalMode", null);
    }
}
