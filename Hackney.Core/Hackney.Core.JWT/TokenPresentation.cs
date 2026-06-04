using System.Text.Json.Serialization;

namespace Hackney.Core.JWT;

/// <summary>
/// Presentation layer object representing what Token could be.
/// A seperate object is needed to avoid breaking the existing
/// Domain layer contract. Do not use outside Hackney.Core.JWT
/// </summary>
public class TokenPresentation
{
    public required string Sub { get; set; }
    // Nullable due to transition period
    // The old token format groups field
    public string[]? Groups { get; set; }

    // Nullable due to transition period
    // New cognito token format field
    [JsonPropertyName("custom:groups")]
    public string? CustomGroups { get; set; }

    public required string Email { get; set; }
    public required string Name { get; set; }
    public required long Nbf { get; set; }
    public required long Exp { get; set; }
    public required long Iat { get; set; }
}
