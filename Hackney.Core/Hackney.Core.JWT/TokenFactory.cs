using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Hackney.Core.JWT;

/// <summary>
/// Parses JWT strings, maps JWT payloads to domain `Token` objects, and provides
/// a lightweight token-type identification helper. Methods are defensive: malformed
/// tokens are logged and cause safe return values (`null` or `Unknown`) instead of
/// throwing for the caller.
/// </summary>
public class TokenFactory : ITokenFactory
{
    // Separator aws cognito pre-token lambda uses to join Google group names with
    private char CognitoTokenGoogleGroupsSeparator => ';';
    // While a version of this with whitespace in between "{}" is possible and would be decoded in much the same way,
    // it's unlikely to happen naturally. Cases like accidentally JSON serializing an unawaited promise instead of data
    // it would return typically return the spaceless version like specified here.
    private string EmptyObjectJsonPayload => "{}";
    private readonly ILogger<TokenFactory> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TokenFactory(ILogger<TokenFactory> logger)
    {
        _logger = logger;
    }

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
    public Token? Create(IHeaderDictionary headerDictionary, string headerName = ITokenFactory.DefaultHeaderName)
    {
        if (headerDictionary is null) throw new ArgumentNullException(nameof(headerDictionary));
        if (string.IsNullOrEmpty(headerName)) throw new ArgumentNullException(nameof(headerName));

        var headerStringValues = headerDictionary[headerName];

        if (headerStringValues.Count == 0)
        {
            _logger.LogWarning("Provided header '{HeaderName}' is empty.", headerName);
            return null;
        }

        if (headerStringValues.Count > 1)
        {
            _logger.LogWarning("Multiple (count: {Count}) header values detected, using the first one.", headerStringValues.Count);
        }

        string? firstHeaderValue = headerStringValues[0];

        if (string.IsNullOrEmpty(firstHeaderValue))
        {
            _logger.LogWarning("First extracted HeaderDictionary StringValues primitive value is empty.");
            return null;
        }

        return DecodeStandardToken(firstHeaderValue);
    }

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

    public Token? DecodeStandardToken(string jwtStr)
    {
        var presentationToken = this.Decode<TokenPresentation>(jwtStr);

        if (presentationToken is null)
        {
            _logger.LogWarning("Failed to deserialize JWT payload.");
            return null;
        }

        return MapToDomain(presentationToken);
    }

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
    public T? Decode<T>(string jwtStr) where T : class
    {
        // Prevent misleading API consumers with 500 Internal Server Errors due to a bad token input. Fail gracefully.
        // Defensive check kept for consumers without nullable contexts, but the signature now enforces intent.
        if (string.IsNullOrEmpty(jwtStr))
        {
            _logger.LogWarning("No JWT token was provided.");
            return null;
        }

        var encodedString = jwtStr.Replace("Bearer ", "", StringComparison.CurrentCultureIgnoreCase);

        try
        {
            JsonWebTokenHandler handler = new();
            var jwtToken = handler.ReadJsonWebToken(encodedString);

            var decodedPayload = Base64UrlEncoder.Decode(jwtToken.EncodedPayload);

            if (decodedPayload == this.EmptyObjectJsonPayload)
            {
                _logger.LogWarning("JWT payload is empty JSON object.");
                return null;
            }

            return JsonSerializer.Deserialize<T>(decodedPayload, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected, Null, or Malformed token: {InvalidToken}.", jwtStr);
            return null;
        }
    }

    /// <summary>
    /// Map the presentation DTO to the legacy domain <see cref="Token"/>, normalising group values.
    /// </summary>
    /// <param name="presentation">The deserialised token presentation DTO.</param>
    /// <returns>The mapped domain <see cref="Token"/>.</returns>
    private Token MapToDomain(TokenPresentation presentation)
    {
        var parsedGroups = presentation.Groups ?? (
            !string.IsNullOrWhiteSpace(presentation.CustomGroups)
                ? presentation.CustomGroups.Split(this.CognitoTokenGoogleGroupsSeparator, StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>()
            );

        return new Token
        {
            Sub = presentation.Sub,
            Groups = parsedGroups,
            Email = presentation.Email,
            Name = presentation.Name,
            Nbf = presentation.Nbf,
            Exp = presentation.Exp,
            Iat = presentation.Iat
        };
    }

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
    public HackneyTokenType IdentifyHackneyToken(string jwtStr)
    {
        var cleanJwt = jwtStr.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        var parts = cleanJwt.Split('.');

        // if signature part is missing or exists, but is empty - it's an unknown token as all Hackney tokens are signed.
        if (parts.Length != 3 || string.IsNullOrEmpty(parts[2]))
            return HackneyTokenType.Unknown;

        try
        {
            var jwtPayload = parts[1];
            var decodedJson = Base64UrlEncoder.Decode(jwtPayload);

            using var document = JsonDocument.Parse(decodedJson);
            var root = document.RootElement;

            if (root.TryGetProperty("consumerName", out _))
                return HackneyTokenType.MachineLegacy;

            if (root.TryGetProperty("email", out _))
                return HackneyTokenType.User;

            return HackneyTokenType.Unknown;
        }
        catch
        {
            return HackneyTokenType.Unknown;
        }
    }
}
