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

    private static string BotanicalPath(string endpoint) => $"{BotanicalController.RoutePrefix}/{endpoint}";
    private static string AquaticPath(string endpoint) => $"{AquaticController.RoutePrefix}/{endpoint}";

    private static HttpRequestMessage CreateRequest(string path, string machineEndpoint)
    {
        var jwt = TestJwt.CognitoM2M(ClientId, $"{ApiName}/{machineEndpoint}.read");
        return TestHttp.BearerGet(path, jwt);
    }

    #endregion

    #region controller attribute is inherited

    [Fact]
    public async Task UndecoratedMethod_AllowsAccess_WhenItInheritsTheControllerAttribute()
    {
        // arrange
        using var request = CreateRequest(
            BotanicalPath(BotanicalController.DandelionEndpointName),
            BotanicalController.MachineAccess);

        // act
        using var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadBody();
        body.Should().Contain("allowed");
    }

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenTheTokenDoesNotMatchTheInheritedControllerAttribute()
    {
        // arrange
        using var request = CreateRequest(
            BotanicalPath(BotanicalController.DandelionEndpointName),
            BotanicalController.ChamomileEndpointName);

        // act
        using var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region method attribute overrides the controller attribute

    [Fact]
    public async Task DecoratedMethod_AllowsAccess_WhenTheTokenMatchesTheOverridingMethodAttribute()
    {
        // arrange
        using var request = CreateRequest(
            BotanicalPath(BotanicalController.ChamomileEndpointName),
            BotanicalController.ChamomileEndpointName);

        // act
        using var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadBody();
        body.Should().Contain("allowed");
    }

    [Fact]
    public async Task DecoratedMethod_DeniesAccess_WhenTheTokenOnlyMatchesTheOverriddenControllerAttribute()
    {
        // arrange
        using var request = CreateRequest(
            BotanicalPath(BotanicalController.ChamomileEndpointName),
            BotanicalController.MachineAccess);

        // act
        using var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region method attribute does not apply to another method

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenAnotherMethodOnTheControllerHasAnAttribute()
    {
        // arrange
        using var request = CreateRequest(
            AquaticPath(AquaticController.GreatCrestedNewtEndpointName),
            AquaticController.AtlanticSalmonEndpointName);

        // act
        using var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region no attribute denies the request

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenTheControllerAlsoHasNoAttribute()
    {
        // arrange
        // The route name is not an access rule. Neither the action nor the controller has AllowAccessTo.
        using var request = CreateRequest(
            AquaticPath(AquaticController.GreatCrestedNewtEndpointName),
            AquaticController.GreatCrestedNewtEndpointName);

        // act
        using var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
