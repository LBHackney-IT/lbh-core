using Amazon;
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

public class DynamoDbInitilisationExtensionsTests : IDisposable
{
    private const string LocalServiceUrl = "http://localhost:8000/";
    private const string LocalModeVariable = "DynamoDb_LocalMode";
    private const string LocalServiceUrlVariable = "DynamoDb_LocalServiceUrl";

    private readonly ServiceCollection _services = new();

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(LocalModeVariable, null);
        Environment.SetEnvironmentVariable(LocalServiceUrlVariable, null);
        AWSConfigs.InitializeCollections = false;
    }

    private static void EnableLocalMode(string serviceUrl = LocalServiceUrl)
    {
        Environment.SetEnvironmentVariable(LocalModeVariable, "true");
        Environment.SetEnvironmentVariable(LocalServiceUrlVariable, serviceUrl);
    }

    private static DynamoDBContextConfig GetContextConfig(IDynamoDBContext context) =>
        (DynamoDBContextConfig)typeof(DynamoDBContext)
            .GetProperty("Config", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(context);

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
        Environment.SetEnvironmentVariable(LocalModeVariable, localModeEnvVar);

        // act
        _services.ConfigureDynamoDB();

        // assert
        var clientRegistration = _services.First(x => x.ServiceType == typeof(IAmazonDynamoDB));
        clientRegistration.Lifetime.Should().Be(ServiceLifetime.Singleton);
        clientRegistration.ImplementationFactory.Should().NotBeNull();

        var contextRegistration = _services.First(x => x.ServiceType == typeof(IDynamoDBContext));
        contextRegistration.Lifetime.Should().Be(ServiceLifetime.Scoped);
        contextRegistration.ImplementationFactory.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureDynamoDB_LocalMode_RegistersClientWithServiceUrlAndContext()
    {
        // arrange
        EnableLocalMode();

        // act
        _services.ConfigureDynamoDB();
        using var serviceProvider = _services.BuildServiceProvider();

        // assert
        var amazonDynamoDB = serviceProvider.GetService<IAmazonDynamoDB>();
        amazonDynamoDB.Should().NotBeNull();
        amazonDynamoDB.Config.ServiceURL.Should().Be(LocalServiceUrl);

        serviceProvider.GetService<IDynamoDBContext>().Should().NotBeNull();
    }

    [Fact]
    public void ConfigureDynamoDB_LeavesExistingClientRegistration()
    {
        // arrange
        EnableLocalMode();
        var existingClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = "http://existing:8000/" });
        _services.AddSingleton<IAmazonDynamoDB>(existingClient);

        // act
        _services.ConfigureDynamoDB();
        using var serviceProvider = _services.BuildServiceProvider();

        // assert
        _services.Count(x => x.ServiceType == typeof(IAmazonDynamoDB)).Should().Be(1);
        serviceProvider.GetService<IAmazonDynamoDB>().Should().BeSameAs(existingClient);
    }

    [Fact]
    public void ConfigureDynamoDB_LeavesExistingContextRegistration()
    {
        // arrange
        EnableLocalMode();
        var existingContext = new Mock<IDynamoDBContext>().Object;
        _services.AddScoped(_ => existingContext);

        // act
        _services.ConfigureDynamoDB();
        using var serviceProvider = _services.BuildServiceProvider();

        // assert
        _services.Count(x => x.ServiceType == typeof(IDynamoDBContext)).Should().Be(1);
        serviceProvider.GetService<IDynamoDBContext>().Should().BeSameAs(existingContext);
    }

    [Fact]
    public void ConfigureDynamoDB_BuiltContext_KeepsTableMetadataFetchingEnabled()
    {
        // arrange
        EnableLocalMode();
        _services.ConfigureDynamoDB();
        using var serviceProvider = _services.BuildServiceProvider();

        // act
        var config = GetContextConfig(serviceProvider.GetService<IDynamoDBContext>());

        // assert
        config.DisableFetchingTableMetadata.Should().BeFalse();
    }

    [Fact]
    public void ConfigureDynamoDB_EnablesInitializeCollections()
    {
        // arrange
        AWSConfigs.InitializeCollections = false;

        // act
        _services.ConfigureDynamoDB();

        // assert
        AWSConfigs.InitializeCollections.Should().BeTrue();
    }
}
