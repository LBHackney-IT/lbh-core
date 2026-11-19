using System.Net.Http;
using System.Threading.Tasks;
using Hackney.Core.Tests.Authorization.E2E.Helpers;
using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E;

[Collection(WildlifeApiCollection.Name)]
public sealed class LegacyMachineAuthorizationTests
{
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

        // act
        var response = await _client.SendBearerGet(WildlifePaths.BarbastellusBat, jwt);

        // assert
        await response.ShouldBeAllowed();
    }

    [Fact]
    public async Task LegacyMachine_DeniesAccess_WhenTheTokenCannotBeDecoded()
    {
        // arrange
        var jwt = TestJwt.UndecodableLegacyMachine("wildlife-survey");

        // act
        var response = await _client.SendBearerGet(WildlifePaths.BarbastellusBat, jwt);

        // assert
        response.ShouldBeForbidden();
    }

    [Fact]
    public async Task LegacyMachine_DeniesAccess_WhenTheEndpointDoesNotAllowLegacyMachine()
    {
        // arrange
        var jwt = TestJwt.LegacyMachine("42", "wildlife-survey", "Service");

        // act
        var response = await _client.SendBearerGet(WildlifePaths.AtlanticSalmon, jwt);

        // assert
        response.ShouldBeForbidden();
    }

    [Fact]
    public async Task LegacyMachine_DeniesAccess_WhenTheAttributeHasNoRules()
    {
        // arrange
        var jwt = TestJwt.LegacyMachine("42", "wildlife-survey", "Service");

        // act
        var response = await _client.SendBearerGet(WildlifePaths.MelesBadger, jwt);

        // assert
        response.ShouldBeForbidden();
    }
}
