using System.Net.Http;
using System.Threading.Tasks;
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
    private readonly HttpClient _client;

    public AttributePlacementAuthorizationTests(WildlifeApiFixture api)
    {
        _client = api.Client;
    }

    #region helper methods

    private Task<HttpResponseMessage> Send(string path, string machineEndpoint)
    {
        var jwt = TestJwt.CognitoM2M(machineEndpoint);
        return _client.SendBearerGet(path, jwt);
    }

    #endregion

    #region controller attribute is inherited

    [Fact]
    public async Task UndecoratedMethod_AllowsAccess_WhenItInheritsTheControllerAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.Dandelion,
            BotanicalController.MachineAccess);

        // assert
        await response.ShouldBeAllowed();
    }

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenTheTokenDoesNotMatchTheInheritedControllerAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.Dandelion,
            BotanicalController.ChamomileEndpointName);

        // assert
        response.ShouldBeForbidden();
    }

    #endregion

    #region method attribute overrides the controller attribute

    [Fact]
    public async Task DecoratedMethod_AllowsAccess_WhenTheTokenMatchesTheOverridingMethodAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.Chamomile,
            BotanicalController.ChamomileEndpointName);

        // assert
        await response.ShouldBeAllowed();
    }

    [Fact]
    public async Task DecoratedMethod_DeniesAccess_WhenTheTokenOnlyMatchesTheOverriddenControllerAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.Chamomile,
            BotanicalController.MachineAccess);

        // assert
        response.ShouldBeForbidden();
    }

    #endregion

    #region method attribute does not apply to another method

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenAnotherMethodOnTheControllerHasAnAttribute()
    {
        // act
        using var response = await Send(
            WildlifePaths.GreatCrestedNewt,
            AquaticController.AtlanticSalmonEndpointName);

        // assert
        response.ShouldBeForbidden();
    }

    #endregion

    #region no attribute denies the request

    [Fact]
    public async Task UndecoratedMethod_DeniesAccess_WhenTheControllerAlsoHasNoAttribute()
    {
        // act
        // The route name is not an access rule. Neither the action nor the controller has AllowAccessTo.
        using var response = await Send(
            WildlifePaths.GreatCrestedNewt,
            AquaticController.GreatCrestedNewtEndpointName);

        // assert
        response.ShouldBeForbidden();
    }

    #endregion
}
