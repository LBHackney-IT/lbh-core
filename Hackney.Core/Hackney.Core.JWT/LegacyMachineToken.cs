using System.Text.Json.Serialization;

namespace Hackney.Core.JWT;

/// <summary>
/// Presentation (and Domain for the time being) model representing the
/// Machine to Machine (M2M) token schema used by the Hackney's legacy
/// "Service" authentication flow.
/// </summary>
public class LegacyMachineToken
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("consumerName")]
    public required string ConsumerName { get; set; }

    [JsonPropertyName("consumerType")]
    public required string ConsumerType { get; set; }

    [JsonPropertyName("nbf")]
    public long? Nbf { get; set; }

    [JsonPropertyName("exp")]
    public long? Exp { get; set; }

    [JsonPropertyName("iat")]
    public long Iat { get; set; }
}
