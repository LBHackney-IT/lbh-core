using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E;

[Collection(WildlifeApiCollection.Name)]
public sealed class UserAuthorizationTests
{
    private static readonly string BatPath =
        $"{AnimalsController.RoutePrefix}/{AnimalsController.BarbastellusBatEndpointName}";

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

        using var request = new HttpRequestMessage(HttpMethod.Get, BatPath);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // act
        var response = await _client.SendAsync(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("allowed");
    }

    [Fact]
    public async Task User_DeniesAccess_WhenTheTokenGroupDoesNotMatchTheEndpointUserPolicy()
    {
        // arrange
        var jwt = TestJwt.User("keeper@hackney.gov.uk", "bird-surveyors");

        using var request = new HttpRequestMessage(HttpMethod.Get, BatPath);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // act
        var response = await _client.SendAsync(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
