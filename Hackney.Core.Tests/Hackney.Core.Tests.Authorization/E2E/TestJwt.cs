using System;
using System.Text;
using System.Text.Json;

namespace Hackney.Core.Tests.Authorization.E2E;

internal static class TestJwt
{
    public static string CognitoM2M(string clientId, string scope)
    {
        var header = Base64Url("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var payload = Base64Url(JsonSerializer.Serialize(new { client_id = clientId, scope }));

        // Identification rejects a missing signature segment. The value itself is not checked.
        return $"{header}.{payload}.signature";
    }

    private static string Base64Url(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
