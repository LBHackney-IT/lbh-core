using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AwesomeAssertions;
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

        using var request = new HttpRequestMessage(HttpMethod.Get, "animals/barbastellus-bat");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // act
        var response = await _client.SendAsync(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("allowed");
    }

    [Fact]
    public async Task CognitoM2M_DeniesAccess_WhenTheTokenScopeDoesNotMatchTheEndpointMachineClaimRestriction()
    {
        // arrange
        var jwt = TestJwt.CognitoM2M("wildlife-consumer", "wildlife-api/other-endpoint.read");

        using var request = new HttpRequestMessage(HttpMethod.Get, "animals/barbastellus-bat");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // act
        var response = await _client.SendAsync(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
