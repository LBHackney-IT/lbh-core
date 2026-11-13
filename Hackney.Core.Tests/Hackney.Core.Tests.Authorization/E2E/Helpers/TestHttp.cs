using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

internal static class TestHttp
{
    // xUnit only recognizes this exact token expression. The tests call these methods instead of repeating it.
    public static Task<HttpResponseMessage> SendRequest(this HttpClient client, HttpRequestMessage request) =>
        client.SendAsync(request, TestContext.Current.CancellationToken);

    public static Task<string> ReadBody(this HttpResponseMessage response) =>
        response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
}
