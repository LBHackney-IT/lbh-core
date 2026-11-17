using System;
using System.Text;
using System.Text.Json;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

internal static class TestJwt
{
    public const string CognitoClientId = "wildlife-consumer";
    public const string ApiName = "wildlife-api";

    public static string CognitoScope(string endpoint) => $"{ApiName}/{endpoint}.read";

    public static string CognitoM2M(string endpoint) =>
        CognitoM2M(CognitoClientId, CognitoScope(endpoint));

    private static string CognitoM2M(string clientId, string scope) =>
        Signed(new { client_id = clientId, scope });

    public static string User(string email, params string[] groups) =>
        Signed(new { sub = "user-1", email, name = "Wildlife Keeper", iat = 1, groups });

    public static string LegacyMachine(string id, string consumerName, string consumerType) =>
        Signed(new { id, consumerName, consumerType, iat = 1 });

    /// Identifies as a legacy machine token, but cannot be decoded: id and consumerType are required.
    public static string UndecodableLegacyMachine(string consumerName) =>
        Signed(new { consumerName });

    private static string Signed(object payload)
    {
        var header = Base64Url("{\"alg\":\"none\",\"typ\":\"JWT\"}");

        // Identification rejects a missing signature segment. The value itself is not checked.
        return $"{header}.{Base64Url(JsonSerializer.Serialize(payload))}.signature";
    }

    private static string Base64Url(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
