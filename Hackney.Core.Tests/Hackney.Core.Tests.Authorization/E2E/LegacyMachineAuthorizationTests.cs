using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E;

[Collection(WildlifeApiCollection.Name)]
public sealed class LegacyMachineAuthorizationTests
{
    private static readonly string BatPath =
        $"{AnimalsController.RoutePrefix}/{AnimalsController.BarbastellusBatEndpointName}";

    private readonly HttpClient _client;

    public LegacyMachineAuthorizationTests(WildlifeApiFixture api)
    {
        _client = api.Client;
    }

    [Fact]
    public async Task LegacyMachine_AllowsAccess_WhenTheTokenCanBeDecoded()
    {
        // arrange
        // The lambda authorizer already checked this token against the endpoint. Decoding it is enough.
        var jwt = TestJwt.LegacyMachine("42", "wildlife-survey", "Service");

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
    public async Task LegacyMachine_DeniesAccess_WhenTheTokenCannotBeDecoded()
    {
        // arrange
        var jwt = TestJwt.UndecodableLegacyMachine("wildlife-survey");

        using var request = new HttpRequestMessage(HttpMethod.Get, BatPath);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // act
        var response = await _client.SendAsync(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
