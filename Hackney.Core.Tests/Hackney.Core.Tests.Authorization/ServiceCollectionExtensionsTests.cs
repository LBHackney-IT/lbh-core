using System;
using System.Collections.Generic;
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
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.Authorization.Internal;
using Hackney.Core.JWT;
using Hackney.Core.Tests.Authorization.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Hackney.Core.Tests.Authorization;

public class ServiceCollectionExtensionsTests
{
    private const string ParameterName = "/auth/groups";
    private static string LoadFailureMessage(string parameterName) =>
        $"Failed to load authorization groups from SSM parameter: '{parameterName}'.";

    #region parameter name

    [Fact]
    public void AddHackneyAuthorization_ThrowsArgumentNullException_WhenParameterNameIsNull()
    {
        // arrange
        var services = new ServiceCollection();
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();

        // act
        var act = () => services.AddHackneyAuthorization(null!, ssm.Object);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("ssmParameterName");
        AssertParameterNameWasRejected(services, ssm);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void AddHackneyAuthorization_ThrowsArgumentException_WhenParameterNameIsBlank(string parameterName)
    {
        // arrange
        var services = new ServiceCollection();
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();

        // act
        var act = () => services.AddHackneyAuthorization(parameterName, ssm.Object);

        // assert
        act.Should().ThrowExactly<ArgumentException>().WithParameterName("ssmParameterName");
        AssertParameterNameWasRejected(services, ssm);
    }

    private static void AssertParameterNameWasRejected(
        ServiceCollection services,
        Mock<IAmazonSimpleSystemsManagement> ssm)
    {
        ssm.Verify(
            client => client.GetParameterAsync(
                It.IsAny<GetParameterRequest>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(ITokenFactory));
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap));
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(IHostedService));
    }

    #endregion

    #region ssm fetch

    [Fact]
    public async Task AddHackneyAuthorization_RequestsTheDecryptedParameterByName_WhenTheHostStarts()
    {
        // arrange
        var ssm = SsmMock.WithPayload(ParameterStorePayload());
        var services = new ServiceCollection();

        // act 1 - registration
        var returned = services.AddHackneyAuthorization(ParameterName, ssm.Object);

        // assert
        returned.Should().BeSameAs(services);
        ssm.Verify(
            client => client.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // act 2 - simulated launch
        await using var provider = services.BuildServiceProvider();
        await StartHostedServices(provider);

        ssm.Verify(
            client => client.GetParameterAsync(
                It.Is<GetParameterRequest>(request => request.Name == ParameterName && request.WithDecryption == true),
                It.Is<CancellationToken>(token => token.CanBeCanceled)
            ),
            Times.Once
        );
        ssm.Verify(client => client.Dispose(), Times.Never);

        provider.GetRequiredService<PolicyGroupMap>().Policies.Should().ContainKey("housing-officers");
    }

    [Fact]
    public async Task AddHackneyAuthorization_ThrowsGroupDocumentLoadException_WhenTheParameterFetchFails()
    {
        // arrange
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();
        var services = new ServiceCollection();

        var ssmFailure = new InvalidOperationException("ssm unavailable");

        ssm.Setup(client => client.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ssmFailure);

        services.AddHackneyAuthorization(ParameterName, ssm.Object);
        await using var provider = services.BuildServiceProvider();

        // act
        var act = async () => await StartHostedServices(provider);

        // assert
        var exception = await act.Should().ThrowAsync<GroupDocumentLoadException>();

        exception.Which.ParameterName.Should().Be(ParameterName);
        exception.Which.Message.Should().Be(LoadFailureMessage(ParameterName));
        exception.Which.InnerException.Should().BeSameAs(ssmFailure);

        provider.GetRequiredService<PolicyGroupMap>().Policies.Should().BeEmpty();
        ssm.Verify(client => client.Dispose(), Times.Never);
    }

    // Currently, this is the only test that constructs the real SSM client. Other tests pass a mock,
    // so their environment cannot conflict over the ClearedAwsDiscovery changes. Should more tests
    // using real client be added, parallelisation may need to be disabled.
    [Fact]
    public async Task AddHackneyAuthorization_ThrowsAmazonClientException_WhenNoSsmClientIsSupplied()
    {
        // arrange
        // no region is visible, so the client the package constructs fails before any network call.
        using var isolatedAws = new ClearedAwsDiscovery();
        var services = new ServiceCollection();
        services.AddHackneyAuthorization(ParameterName);
        await using var provider = services.BuildServiceProvider();

        // act
        var act = async () => await StartHostedServices(provider);

        // assert
        await act.Should().ThrowAsync<AmazonClientException>();

        provider.GetRequiredService<PolicyGroupMap>().Policies.Should().BeEmpty();
    }

    #endregion

    #region group map

    [Fact]
    public async Task AddHackneyAuthorization_RegistersTheGroupMapAsASingleton()
    {
        // arrange
        await using var provider = await BuildProviderAndStartHostedServices(ParameterStorePayload());

        // act
        var policyGroupsMap = provider.GetRequiredService<PolicyGroupMap>();

        // assert
        // 2nd resolve must return the same map. A transient registration creates a new one.
        // sadly doesn't prove it's singleton, but it's the next best thing to proving it does
        // not get needlessly recreated all the time.
        provider.GetRequiredService<PolicyGroupMap>().Should().BeSameAs(policyGroupsMap);
    }

    [Fact]
    public async Task GroupDocumentLoader_ReplacesTheEmptyMap_WhenTheHostStarts()
    {
        // arrange
        var services = new ServiceCollection();
        var ssmMock = SsmMock.WithPayload(ParameterStorePayload()).Object;

        services.AddHackneyAuthorization(ParameterName, ssmMock);
        await using var provider = services.BuildServiceProvider();

        var map = provider.GetRequiredService<PolicyGroupMap>();
        map.Policies.Should().BeEmpty();

        // act
        await StartHostedServices(provider);

        // assert
        provider.GetRequiredService<PolicyGroupMap>().Should().BeSameAs(map);
        map.Policies.Should().ContainKey("housing-officers");
        map.Policies["housing-officers"].Should().BeEquivalentTo("group-a", "Group-A");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("\"housing-officers\"")]
    [InlineData("{ \"wildlife-api\": null }")]
    public async Task AddHackneyAuthorization_ThrowsGroupDocumentLoadException_WhenThePayloadIsNotAGroupDocument(string payload)
    {
        // arrange
        var services = new ServiceCollection();
        var ssm = SsmMock.WithPayload(payload);
        services.AddHackneyAuthorization(ParameterName, ssm.Object);
        await using var provider = services.BuildServiceProvider();

        // act
        var act = async () => await StartHostedServices(provider);

        // assert
        var exception = await act.Should().ThrowAsync<GroupDocumentLoadException>();

        exception.Which.ParameterName.Should().Be(ParameterName);
        exception.Which.Message.Should().Be(LoadFailureMessage(ParameterName));
        exception.Which.InnerException.Should().BeOfType<InvalidGroupDocumentException>();
        provider.GetRequiredService<PolicyGroupMap>().Policies.Should().BeEmpty();
    }

    #endregion

    #region service registration

    [Fact]
    public async Task AddHackneyAuthorization_RegistersTheTokenFactory_FallbackHandler_HybridPolicy_AndGroupsDocumentLoader()
    {
        // arrange
        await using var provider = await BuildProviderAndStartHostedServices(ParameterStorePayload());

        // act
        // token factory is currently scoped, so it has to be resolved from scope. scope is discarded at the end of test
        using var scope = provider.CreateScope();

        var tokenFactory = scope.ServiceProvider.GetRequiredService<ITokenFactory>();
        var authHandler = provider.GetServices<IAuthorizationHandler>().OfType<HybridAccessHandler>().Single();
        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var hybridPolicy = options.GetPolicy(AllowAccessToAttribute.PolicyName);

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

        provider.GetServices<IHostedService>().Should().ContainSingle()
            .Which.Should().BeOfType<GroupDocumentLoader>();
    }

    [Fact]
    public async Task AddHackneyAuthorization_KeepsTheFirstRegistration_WhenCalledTwice()
    {
        // arrange
        var services = new ServiceCollection();
        var ssm = SsmMock.WithPayload(ParameterStorePayload());
        var first = services.AddHackneyAuthorization(ParameterName, ssm.Object);

        // act
        var second = services.AddHackneyAuthorization("/other/groups", ssm.Object);
        await using var provider = services.BuildServiceProvider();
        await StartHostedServices(provider);

        // assert
        second.Should().BeSameAs(first);
        services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(IHostedService));
        services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap));

        ssm.Verify(
            client => client.GetParameterAsync(
                It.Is<GetParameterRequest>(request => request.Name == ParameterName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void AddHackneyAuthorization_ReplacesAnAuthorizationResultHandlerRegisteredEarlier()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, EarlierResultHandler>();

        // act
        services.AddHackneyAuthorization(ParameterName, SsmMock.WithPayload(ParameterStorePayload()).Object);

        // assert
        using var provider = services.BuildServiceProvider();

        provider.GetServices<IAuthorizationMiddlewareResultHandler>().Should().ContainSingle()
            .Which.Should().BeOfType<ForbiddenAuthorizationResultHandler>();
    }

    // mock authorization handler to simulate the scenario where this package's middleware gets
    // registered after the consumer application has already done some startup authorization service
    // registration steps (such as registering a different result handler)
    private sealed class EarlierResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult) => Task.CompletedTask;
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
        await using var provider = await BuildProviderAndStartHostedServices(ParameterStorePayload());

        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var fallback = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;

        // the details of authenticated user are irrelevant as this test is testing whether behaviour stays
        // as intended in a scenario that should not happen in practice under the normal API's setups.
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
        await using var provider = await BuildProviderAndStartHostedServices(ParameterStorePayload());

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

    private static async Task<ServiceProvider> BuildProviderAndStartHostedServices(string payload)
    {
        var services = new ServiceCollection();
        services.AddHackneyAuthorization(ParameterName, SsmMock.WithPayload(payload).Object);
        var provider = services.BuildServiceProvider();
        await StartHostedServices(provider);
        return provider;
    }

    private static async Task StartHostedServices(ServiceProvider provider)
    {
        foreach (var hostedService in provider.GetServices<IHostedService>())
            await hostedService.StartAsync(CancellationToken.None);
    }

    #endregion
}
