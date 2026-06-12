using System.Text.Json.Serialization;

namespace Hackney.Core.JWT;

/// <summary>
/// Identifies the kind of Hackney JWT by inspecting payload markers. This is needed because
/// the APIs consuming tokens need to support 2 distinct token types and perform different
/// authorization logic depending on whether the token belongs to a User or a Machine (Service).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HackneyTokenType
{
    /// <summary>
    /// Token issued to or representing an end user (commonly contains an <c>email</c> claim).
    /// </summary>
    User,

    /// <summary>
    /// Legacy machine-to-machine token (contains a <c>consumerName</c> claim).
    /// </summary>
    MachineLegacy,

    /// <summary>
    /// Token that could not be identified (unsigned, malformed, or otherwise unrecognised).
    /// </summary>
    Unknown
}
