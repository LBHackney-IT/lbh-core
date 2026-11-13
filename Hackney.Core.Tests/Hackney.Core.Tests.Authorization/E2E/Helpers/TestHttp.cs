using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

// helper so that the tests code wouldn't need to repeat these verbose cancellation tokens
internal static class TestHttp
{
    public static Task<HttpResponseMessage> SendRequest(this HttpClient client, HttpRequestMessage request) =>
        client.SendAsync(request, TestContext.Current.CancellationToken);

    public static Task<string> ReadBody(this HttpResponseMessage response) =>
        response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
}
