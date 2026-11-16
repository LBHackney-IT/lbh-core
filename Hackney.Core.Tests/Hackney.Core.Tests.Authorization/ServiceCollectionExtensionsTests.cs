using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using AwesomeAssertions;
using Hackney.Core.Authorization;
using Hackney.Core.Authorization.Internal;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Hackney.Core.Tests.Authorization;

public class ServiceCollectionExtensionsTests
{
    private const string ParameterName = "/auth/groups";
    private const string HybridAccessPolicyName = "HybridAccessPolicy";
    private const string SsmFailureMessagePrefix = "Failed to fetch authorization groups from SSM parameter: ";
    private const string PayloadFailureMessage = "SSM Parameter payload could not be deserialized.";

    #region parameter name

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task AddHackneyAuthorizationAsync_ThrowsArgumentNullException_WhenParameterNameIsMissing(string? parameterName)
    {
        // arrange
        var services = new ServiceCollection();
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();

        // act
        var act = async () => await services.AddHackneyAuthorizationAsync(parameterName!, ssm.Object);

        // assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("ssmParameterName");

        ssm.Verify(
            client => client.GetParameterAsync(
                It.IsAny<GetParameterRequest>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(ITokenFactory));
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap));
    }

    #endregion

    #region ssm fetch

    [Fact]
    public async Task AddHackneyAuthorizationAsync_RequestsTheDecryptedParameterByName()
    {
        // arrange
        var ssm = MockSsm(ParameterStorePayload());
        var services = new ServiceCollection();

        // act
        var returnedParam = await services.AddHackneyAuthorizationAsync(ParameterName, ssm.Object);

        // assert
        returnedParam.Should().BeSameAs(services);
        ssm.Verify(
            client => client.GetParameterAsync(
                It.Is<GetParameterRequest>(request => request.Name == ParameterName && request.WithDecryption == true),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task AddHackneyAuthorizationAsync_ThrowsInvalidOperationException_WhenTheParameterFetchFails()
    {
        // arrange
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();
        var services = new ServiceCollection();

        var ssmFailure = new InvalidOperationException("ssm unavailable");

        ssm.Setup(client => client.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ssmFailure);

        // act
        var act = async () => await services.AddHackneyAuthorizationAsync(ParameterName, ssm.Object);

        // assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should().Be($"{SsmFailureMessagePrefix}'{ParameterName}'");
        exception.Which.InnerException.Should().BeSameAs(ssmFailure);

        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap));
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(IAuthorizationHandler));
    }

    [Fact]
    public async Task AddHackneyAuthorizationAsync_ThrowsAmazonClientException_WhenNoSsmClientIsSupplied()
    {
        // arrange
        // no region is visible, so the client the package constructs fails before any network call.
        using var isolatedAws = new ClearedAwsDiscovery();
        var services = new ServiceCollection();

        // act
        var act = async () => await services.AddHackneyAuthorizationAsync(ParameterName);

        // assert
        var exception = await act.Should().ThrowAsync<AmazonClientException>();

        exception.Which.Message.Should().Be("No RegionEndpoint or ServiceURL configured");

        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap));
    }

    #endregion

    #region group map

    [Fact]
    public async Task AddHackneyAuthorizationAsync_StoresPolicyKeysIgnoringCase_AndGroupsExactly()
    {
        // arrange
        await using var provider = await BuildServiceProvider(ParameterStorePayload());

        // act
        var policyGroupsMap = provider.GetRequiredService<PolicyGroupMap>();

        // assert
        // key comparison
        policyGroupsMap.Policies.Comparer.Should().Be(StringComparer.OrdinalIgnoreCase);
        policyGroupsMap.Policies.Should().ContainKey("housing-officers");

        // key value (groups) comparison
        policyGroupsMap.Policies["HOUSING-OFFICERS"].Comparer.Should().Be(StringComparer.Ordinal);
        policyGroupsMap.Policies["housing-officers"].Should().BeEquivalentTo("group-a", "Group-A");
        policyGroupsMap.Policies["housing-officers"].Contains("GROUP-A").Should().BeFalse();
        policyGroupsMap.Policies["machine-readers"].Should().BeEquivalentTo("reader");

        // 2nd resolve must return the same map. A transient registration creates a new one.
        // sadly doesn't prove it's singleton, but it's the next best thing to proving it does
        // not get needlessly recreated all the time.
        provider.GetRequiredService<PolicyGroupMap>().Should().BeSameAs(policyGroupsMap);
    }

    [Fact]
    public async Task AddHackneyAuthorizationAsync_StoresAnEmptyGroupMap_WhenTheParameterDocumentIsEmpty()
    {
        // arrange
        await using var provider = await BuildServiceProvider("{}");

        // act
        var policyGroupsMap = provider.GetRequiredService<PolicyGroupMap>();

        // assert
        policyGroupsMap.Policies.Should().BeEmpty();
    }

    [Fact]
    public async Task AddHackneyAuthorizationAsync_ThrowsInvalidOperationException_WhenThePayloadIsJsonNull()
    {
        // arrange
        var services = new ServiceCollection();
        var ssm = MockSsm("null");

        // act
        var act = async () => await services.AddHackneyAuthorizationAsync(ParameterName, ssm.Object);

        // assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should().Be(PayloadFailureMessage);
        exception.Which.InnerException.Should().BeNull();

        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("\"housing-officers\"")]
    public async Task AddHackneyAuthorizationAsync_ThrowsJsonException_WhenThePayloadIsNotAGroupDocument(string payload)
    {
        // arrange
        var services = new ServiceCollection();
        var ssm = MockSsm(payload);

        // act
        var act = async () => await services.AddHackneyAuthorizationAsync(ParameterName, ssm.Object);

        // assert
        await act.Should().ThrowAsync<JsonException>();

        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap));
    }

    #endregion

    #region service registration

    [Fact]
    public async Task AddHackneyAuthorizationAsync_RegistersTheTokenFactoryHandlerAndHybridPolicy()
    {
        // arrange
        await using var provider = await BuildServiceProvider(ParameterStorePayload());

        // act
        // token factory is currently scoped, so it has to be resolved from scope. scope is discarded at the end of test
        using var scope = provider.CreateScope();

        var tokenFactory = scope.ServiceProvider.GetRequiredService<ITokenFactory>();
        var authHandler = provider.GetServices<IAuthorizationHandler>().OfType<HybridAccessHandler>().Single();
        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var hybridPolicy = options.GetPolicy(HybridAccessPolicyName);

        // assert
        tokenFactory.Should().BeOfType<TokenFactory>();

        // same instance - returns cached handler
        authHandler.Should().BeSameAs(
            provider.GetServices<IAuthorizationHandler>().OfType<HybridAccessHandler>().Single()
        );

        hybridPolicy.Should().NotBeNull();
        hybridPolicy!.Requirements.Should().ContainSingle()
            .Which.Should().BeOfType<HybridAccessRequirement>();

        options.FallbackPolicy.Should().NotBeNull();
        options.FallbackPolicy.Should().NotBeSameAs(hybridPolicy);

        provider.GetRequiredService<IAuthorizationMiddlewareResultHandler>()
            .Should().BeOfType<ForbiddenAuthorizationResultHandler>();
    }

    #endregion

    #region fallback policy

    // Counter-intuitive for why this is desired. It's needed to plug the security gap in a fallback policy setup.
    // In short, the fallback policy is added to ensure that if developers were to forget to decorate a new
    // endpoint with an authorization attribute, that this endpoint would NOT allow ALL traffic to it that
    // has reached an API. The ASP.NET has alternative policies that can be set as fallback, that add the
    // conditionallity that we do not want for this case. This test checks against one such case of conditionallity.
    [Fact]
    public async Task FallbackPolicy_DeniesAnAuthenticatedCaller()
    {
        // arrange
        await using var provider = await BuildServiceProvider(ParameterStorePayload());

        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var fallback = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;

        var caller = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "caller")],
            authenticationType: "Bearer"));

        // act
        var result = await authorization.AuthorizeAsync(caller, fallback!);

        // assert
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task FallbackPolicy_DeniesAnAnonymousCaller()
    {
        // arrange
        await using var provider = await BuildServiceProvider(ParameterStorePayload());

        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var fallback = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;

        // act
        var result = await authorization.AuthorizeAsync(new ClaimsPrincipal(), fallback!);

        // assert
        result.Succeeded.Should().BeFalse();
    }

    #endregion

    #region helper methods

    private static string ParameterStorePayload() => JsonSerializer.Serialize(new Dictionary<string, string[]>
    {
        ["Housing-Officers"] = ["group-a", "Group-A"],
        ["machine-readers"] = ["reader"]
    });

    private static Mock<IAmazonSimpleSystemsManagement> MockSsm(string payload)
    {
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();

        ssm.Setup(client => client.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse
            {
                Parameter = new Parameter { Value = payload }
            });

        return ssm;
    }

    private static async Task<ServiceProvider> BuildServiceProvider(string payload)
    {
        var services = new ServiceCollection();
        await services.AddHackneyAuthorizationAsync(ParameterName, MockSsm(payload).Object);
        return services.BuildServiceProvider();
    }

    // TODO: move to a helper file
    /// <summary>
    /// Hide the developer machine's AWS profile, region, and metadata service so the
    /// parameter-store client constructed by the package fails before it opens a connection.
    /// </summary>
    private sealed class ClearedAwsDiscovery : IDisposable
    {
        private readonly Dictionary<string, string?> _previous = [];
        private readonly string _configPath = Path.GetTempFileName();
        private readonly string _credentialsPath = Path.GetTempFileName();

        public ClearedAwsDiscovery()
        {
            Set("AWS_REGION", null);
            Set("AWS_DEFAULT_REGION", null);
            Set("AWS_PROFILE", null);
            Set("AWS_ACCESS_KEY_ID", "test-key");
            Set("AWS_SECRET_ACCESS_KEY", "test-secret");
            Set("AWS_SESSION_TOKEN", null);
            Set("AWS_EC2_METADATA_DISABLED", "true");
            Set("AWS_CONTAINER_CREDENTIALS_RELATIVE_URI", null);
            Set("AWS_CONTAINER_CREDENTIALS_FULL_URI", null);
            Set("AWS_SHARED_CREDENTIALS_FILE", _credentialsPath);
            Set("AWS_CONFIG_FILE", _configPath);
        }

        public void Dispose()
        {
            foreach (var (name, value) in _previous)
                Environment.SetEnvironmentVariable(name, value);

            File.Delete(_configPath);
            File.Delete(_credentialsPath);
        }

        private void Set(string name, string? value)
        {
            _previous[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    #endregion
}
