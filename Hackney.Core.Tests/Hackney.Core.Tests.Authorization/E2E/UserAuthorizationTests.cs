using System.Net.Http;
using System.Threading.Tasks;
using Hackney.Core.Tests.Authorization.E2E.Helpers;
using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E;

[Collection(WildlifeApiCollection.Name)]
public sealed class UserAuthorizationTests
{
    private readonly HttpClient _client;

    public UserAuthorizationTests(WildlifeApiFixture api)
    {
        _client = api.Client;
    }

    [Fact]
    public async Task User_AllowsAccess_WhenTheTokenGroupMatchesTheEndpointUserPolicy()
    {
        // arrange
        var jwt = TestJwt.User("keeper@hackney.gov.uk", AnimalsController.UserGroup);

        // act
        var response = await _client.SendBearerGet(WildlifePaths.BarbastellusBat, jwt);

        // assert
        await response.ShouldBeAllowed();
    }

    [Fact]
    public async Task User_DeniesAccess_WhenTheTokenGroupDoesNotMatchTheEndpointUserPolicy()
    {
        // arrange
        var jwt = TestJwt.User("keeper@hackney.gov.uk", "bird-surveyors");

        // act
        var response = await _client.SendBearerGet(WildlifePaths.BarbastellusBat, jwt);

        // assert
        response.ShouldBeForbidden();
    }

    [Fact]
    public async Task User_DeniesAccess_WhenTheAttributeHasNoRules()
    {
        // arrange
        var jwt = TestJwt.User("keeper@hackney.gov.uk", AnimalsController.UserGroup);

        // act
        var response = await _client.SendBearerGet(WildlifePaths.MelesBadger, jwt);

        // assert
        response.ShouldBeForbidden();
    }
}
