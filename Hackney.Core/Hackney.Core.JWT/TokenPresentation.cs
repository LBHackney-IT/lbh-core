using System.Text.Json.Serialization;

namespace Hackney.Core.JWT;

/// <summary>
/// Presentation layer object representing what Token could be.
/// A seperate object is needed to avoid breaking the existing
/// Domain layer contract. Do not use outside Hackney.Core.JWT
/// </summary>
public class TokenPresentation
{
    [JsonPropertyName("sub")]
    public required string Sub { get; set; }

    // Nullable due to transition period
    // The old token format groups field
    [JsonPropertyName("groups")]
    public string[]? Groups { get; set; }

    // Nullable due to transition period
    // New cognito token format field
    [JsonPropertyName("custom:groups")]
    public string? CustomGroups { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    // Nbf and Exp would ideally be required, that's not compatible with the Legacy token
    [JsonPropertyName("nbf")]
    public long? Nbf { get; set; }

    [JsonPropertyName("exp")]
    public long? Exp { get; set; }

    [JsonPropertyName("iat")]
    public required long Iat { get; set; }
}
