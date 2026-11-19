using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AwesomeAssertions;
using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

internal static class TestHttp
{
    public static async Task<HttpResponseMessage> SendGet(this HttpClient client, string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        return await client.SendRequest(request);
    }

    public static async Task<HttpResponseMessage> SendBearerGet(this HttpClient client, string path, string jwt)
    {
        using var request = BearerGet(path, jwt);
        return await client.SendRequest(request);
    }

    private static HttpRequestMessage BearerGet(string path, string jwt)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return request;
    }

    private static Task<HttpResponseMessage> SendRequest(this HttpClient client, HttpRequestMessage request) =>
        client.SendAsync(request, TestContext.Current.CancellationToken);

    public static async Task ShouldBeAllowed(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadBody();
        body.Should().Contain(WildlifeApiControllerBase.AllowedStatus);
    }

    public static void ShouldBeForbidden(this HttpResponseMessage response) =>
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

    private static Task<string> ReadBody(this HttpResponseMessage response) =>
        response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
}
