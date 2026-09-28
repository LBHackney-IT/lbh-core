using System.Text.Json.Serialization;

namespace Hackney.Core.JWT;

/// <summary>
/// Presentation layer model for the Cognito machine-to-machine access token.
/// Only expected to be used fields are specified. Under our authorization
/// model, the JTW fields should be verified by the lambda authorizer.
/// </summary>
internal class CognitoM2MTokenPresentation
{
    /// <summary>
    /// Specific Cognito app client's identifier. Since one app client is
    /// expected per machine consumer, this should uniquely identify a consumer.
    /// </summary>
    [JsonPropertyName("client_id")]
    public required string ClientId { get; set; }

    /// <summary>
    /// Cognito injects access scopes under a 'scope' key as a space-separated
    /// list of strings matching claims specified on the Cognito resource server.
    /// In our case, the format of an individual scope is:
    /// {apiGatewayId}/{endpointName}.{accessType}, e.g. "2524go3mdg/tenures.post"
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
