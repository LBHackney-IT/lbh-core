using System.Net.Http;
using System.Threading.Tasks;
using Hackney.Core.Tests.Authorization.E2E.Helpers;
using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E;

[Collection(WildlifeApiCollection.Name)]
public sealed class CognitoM2MAuthorizationTests
{
    private readonly HttpClient _client;

    public CognitoM2MAuthorizationTests(WildlifeApiFixture api)
    {
        _client = api.Client;
    }

    [Fact]
    public async Task CognitoM2M_AllowsAccess_WhenTheTokenScopeMatchesTheEndpointMachineClaimRestriction()
    {
        // arrange
        var jwt = TestJwt.CognitoM2M("wildlife-consumer", $"wildlife-api/{AnimalsController.BarbastellusBatEndpointName}.read");

        // act
        var response = await _client.SendBearerGet(WildlifePaths.BarbastellusBat, jwt);

        // assert
        await response.ShouldBeAllowed();
    }

    [Fact]
    public async Task CognitoM2M_DeniesAccess_WhenTheTokenScopeDoesNotMatchTheEndpointMachineClaimRestriction()
    {
        // arrange
        var jwt = TestJwt.CognitoM2M("wildlife-consumer", "wildlife-api/other-endpoint.read");

        // act
        var response = await _client.SendBearerGet(WildlifePaths.BarbastellusBat, jwt);

        // assert
        response.ShouldBeForbidden();
    }
}
