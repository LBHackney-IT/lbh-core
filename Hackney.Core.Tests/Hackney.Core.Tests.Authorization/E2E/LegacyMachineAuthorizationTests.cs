using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using Hackney.Core.Tests.Authorization.E2E.Helpers;
using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;
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
        using var request = TestHttp.BearerGet(BatPath, jwt);

        // act
        var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadBody();
        body.Should().Contain("allowed");
    }

    [Fact]
    public async Task LegacyMachine_DeniesAccess_WhenTheTokenCannotBeDecoded()
    {
        // arrange
        var jwt = TestJwt.UndecodableLegacyMachine("wildlife-survey");
        using var request = TestHttp.BearerGet(BatPath, jwt);

        // act
        var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LegacyMachine_DeniesAccess_WhenTheEndpointDoesNotAllowLegacyMachine()
    {
        // arrange
        var jwt = TestJwt.LegacyMachine("42", "wildlife-survey", "Service");
        var salmonPath = $"{AquaticController.RoutePrefix}/{AquaticController.AtlanticSalmonEndpointName}";
        using var request = TestHttp.BearerGet(salmonPath, jwt);

        // act
        var response = await _client.SendRequest(request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
