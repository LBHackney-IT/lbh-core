using System;
using Microsoft.AspNetCore.Http;

namespace Hackney.Core.JWT;

/// <summary>
/// Parses JWT strings, maps JWT payloads to domain `Token` objects, and provides
/// a lightweight token-type identification helper. Methods are defensive: malformed
/// tokens are logged and cause safe return values (`null` or `Unknown`) instead of
/// throwing for the caller.
/// </summary>
public interface ITokenFactory
{
    /// <summary>
    /// The default Http header name used to create a token
    /// </summary>
    public const string DefaultHeaderName = "Authorization";

    /// <summary>
    /// Creates a domain <see cref="Token"/> by extracting a JWT from the specified header and decoding it.
    /// </summary>
    /// <param name="headerDictionary">Request headers.</param>
    /// <param name="headerName">Header key to read (default: <c>Authorization</c>).</param>
    /// <returns>
    /// The decoded <see cref="Token"/>, or <c>null</c> when the header is missing, empty,
    /// or decoding fails. Decoding failures are logged rather than thrown.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="headerDictionary"/> is null or <paramref name="headerName"/> is null or empty.</exception>
    [Obsolete("Deprecated to decouple token parsing from HTTP abstractions. This method will be removed entirely in the next major version. Extract the token string in the consuming API and use Decode instead.", error: false, DiagnosticId = "HACKNEY_DEPRECATED_TOKEN_CREATE")]
    Token? Create(IHeaderDictionary headerDictionary, string headerName = DefaultHeaderName);

    /// <summary>
    /// Decodes a JWT string from both Legacy and Cognito schemas and maps it to the Domain <see cref="Token"/> schema.
    /// </summary>
    /// <param name="jwtStr">JWT string (may include a leading <c>Bearer </c> prefix). Note! If a <c>IHeaderDictionary</c>
    /// header value of type <c>Microsoft.Extensions.Primitives.StringValues</c> is passed then an implicit conversion of
    /// <c>StringValues.ToString()</c> will get triggered to concatinate all header string values into a single string
    /// (see https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.primitives.stringvalues.tostring).</param>
    /// <returns>
    /// A mapped <see cref="Token"/>, or <c>null</c> when decoding/deserialization fails
    /// or the payload is an empty JSON object. Failures are logged.
    /// </returns>
    Token? DecodeStandardToken(string jwtStr);

    /// <summary>
    /// Decodes the JWT payload and deserializes it into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize the JWT payload into.</typeparam>
    /// <param name="jwtStr">The JWT string to decode. May be prefixed with <c>Bearer </c>.</param>
    /// <returns>
    /// An instance of <typeparamref name="T"/> if decoding and deserialization succeed; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method strips an optional <c>Bearer </c> prefix, reads the token using
    /// <see cref="Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler"/>, base64url-decodes
    /// the payload and deserializes it with <c>System.Text.Json</c> using case-insensitive
    /// property name matching. Malformed tokens, empty JSON object payloads, or
    /// deserialization errors cause the method to log a warning and return <c>null</c>.
    /// </remarks>
    T? Decode<T>(string jwtStr) where T : class;

    /// <summary>
    /// Efficiently peeks into the JWT payload without full deserialization to identify the token type based on structural markers.
    /// </summary>
    /// <param name="jwtStr">JWT string to inspect. Leading <c>Bearer </c> prefix is accepted.</param>
    /// <returns>
    /// A <see cref="HackneyTokenType"/> value: <c>MachineLegacy</c> when payload contains <c>consumerName</c>,
    /// <c>User</c> when payload contains <c>email</c>, otherwise <c>Unknown</c>.
    /// </returns>
    /// <remarks>
    /// Expects a three-part JWT with a non-empty signature segment. Two-part tokens
    /// (header.payload) or tokens with an empty signature (header.payload.) are treated
    /// as unsigned and return <c>Unknown</c>. Any parsing errors also return <c>Unknown</c>.
    /// </remarks>
    HackneyTokenType IdentifyHackneyToken(string jwtStr);
}
