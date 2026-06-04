using System.Text.Json.Serialization;

namespace Hackney.Core.JWT;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HackneyTokenType
{
    User,
    MachineLegacy,
    Unknown
}
