using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Hackney.Core.JWT;

/// <inheritdoc/>
public class TokenFactory : ITokenFactory
{
    // Separator aws cognito pre-token lambda uses to join Google group names with
    private static char CognitoTokenGoogleGroupsSeparator => ';';
    // While a version of this with whitespace in between "{}" is possible and would be decoded in much the same way,
    // it's unlikely to happen naturally. Cases like accidentally JSON serializing an unawaited promise instead of data
    // it would return typically return the spaceless version like specified here.
    private static string EmptyObjectJsonPayload => "{}";
    private static int LoggedTokenTruncateLimit => 30;
    private readonly ILogger<TokenFactory> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TokenFactory(ILogger<TokenFactory> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    [Obsolete("Deprecated to decouple token parsing from HTTP abstractions. This method will be removed entirely in the next major version. Extract the token string in the consuming API and use Decode instead.", error: false, DiagnosticId = "HACKNEY_DEPRECATED_TOKEN_CREATE")]
    public Token? Create(IHeaderDictionary headerDictionary, string headerName = ITokenFactory.DefaultHeaderName)
    {
        ArgumentNullException.ThrowIfNull(headerDictionary);
        ArgumentException.ThrowIfNullOrEmpty(headerName);

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

    /// <inheritdoc/>
    public Token? DecodeStandardToken(string jwtStr)
    {
        var presentationToken = Decode<TokenPresentation>(jwtStr);

        if (presentationToken is null)
        {
            _logger.LogWarning("Failed to deserialize JWT payload.");
            return null;
        }

        return MapToDomain(presentationToken);
    }

    /// <inheritdoc/>
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

            if (decodedPayload == EmptyObjectJsonPayload)
            {
                _logger.LogWarning("JWT payload is empty JSON object.");
                return null;
            }

            return JsonSerializer.Deserialize<T>(decodedPayload, _jsonOptions);
        }
        catch (Exception ex)
        {
            var truncatedToken = TruncateLoggedToken(jwtStr);
            _logger.LogWarning(ex, "Unexpected, Null, or Malformed token: {InvalidToken}.", truncatedToken);
            return null;
        }
    }

    private string TruncateLoggedToken(string? jwtStr)
    {
        string sanitizedToken = (jwtStr ?? "null").Replace("\r", "").Replace("\n", "");
        // Truncating to prevent leaking into logs in case it's a proper token with unexpected characters attached.
        return sanitizedToken.Length > LoggedTokenTruncateLimit
            ? $"{sanitizedToken[..LoggedTokenTruncateLimit]}..."
            : sanitizedToken;
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
                ? presentation.CustomGroups.Split(CognitoTokenGoogleGroupsSeparator, StringSplitOptions.RemoveEmptyEntries)
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

    /// <inheritdoc/>
    public HackneyTokenType IdentifyHackneyToken(string jwtStr)
    {
        var cleanJwt = jwtStr.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        var parts = cleanJwt.Split('.');

        // if signature part is missing or exists, but is empty - it's an unknown token as all Hackney tokens are signed.
        if (parts.Length != 3 || string.IsNullOrEmpty(parts[2]))
        {
            _logger.LogWarning("JWT is not signed. Hackney tokens are always signed.");
            return HackneyTokenType.Unknown;
        }

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

            _logger.LogWarning("Provided JWT did not match any known and expected token schema fields.");
            return HackneyTokenType.Unknown;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token identification failed due to error or unknown token format.");
            return HackneyTokenType.Unknown;
        }
    }
}
