using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using Hackney.Core.Tests.Authorization.E2E.Helpers;
using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E;

/// <summary>
/// E2e Tests for how <c>AllowAccessTo</c> on a controller combines with <c>AllowAccessTo</c> on an endpoint method (action).
/// The action attribute replaces the controller attribute. An action with neither is meant to be denied by default.
/// </summary>
[Collection(WildlifeApiCollection.Name)]
public sealed class AttributePlacementAuthorizationTests
{
    private const string ClientId = "wildlife-consumer";
    private const string ApiName = "wildlife-api";

    private readonly HttpClient _client;

    public AttributePlacementAuthorizationTests(WildlifeApiFixture api)
    {
        _client = api.Client;
    }

    #region helper methods

    private Task<HttpResponseMessage> Send(string path, string machineEndpoint)
    {
        var jwt = TestJwt.CognitoM2M(ClientId, $"{ApiName}/{machineEndpoint}.read");
        return _client.SendBearerGet(path, jwt);
    }

    #endregion

    #region controller attribute is inherited

    [Fact]
    public async Task UndecoratedMethod_AllowsAccess_WhenItInheritsTheControllerAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.BotanicalPath(BotanicalController.DandelionEndpointName),
            BotanicalController.MachineAccess);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadBody();
        body.Should().Contain("allowed");
    }

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenTheTokenDoesNotMatchTheInheritedControllerAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.BotanicalPath(BotanicalController.DandelionEndpointName),
            BotanicalController.ChamomileEndpointName);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region method attribute overrides the controller attribute

    [Fact]
    public async Task DecoratedMethod_AllowsAccess_WhenTheTokenMatchesTheOverridingMethodAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.BotanicalPath(BotanicalController.ChamomileEndpointName),
            BotanicalController.ChamomileEndpointName);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadBody();
        body.Should().Contain("allowed");
    }

    [Fact]
    public async Task DecoratedMethod_DeniesAccess_WhenTheTokenOnlyMatchesTheOverriddenControllerAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.BotanicalPath(BotanicalController.ChamomileEndpointName),
            BotanicalController.MachineAccess);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region method attribute does not apply to another method

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenAnotherMethodOnTheControllerHasAnAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.AquaticPath(AquaticController.GreatCrestedNewtEndpointName),
            AquaticController.AtlanticSalmonEndpointName);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region no attribute denies the request

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenTheControllerAlsoHasNoAttribute()
    {
        // act
        // The route name is not an access rule. Neither the action nor the controller has AllowAccessTo.
        using var response = await Send(
            WildlifePaths.AquaticPath(AquaticController.GreatCrestedNewtEndpointName),
            AquaticController.GreatCrestedNewtEndpointName);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
