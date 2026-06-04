using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Hackney.Core.JWT;

/// <summary>
/// Class implementing the creation of a token object from a JWT included in Http headers
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
    /// Extracts a JWT from the supplied Http headers and creates a token object from it.
    /// </summary>
    /// <param name="headerDictionary">The Http headers</param>
    /// <param name="headerName">The header key name used. Default: "Authorization"</param>
    /// <returns>The deserialised Token or null</returns>
    /// <exception cref="System.ArgumentNullException">If the headerDictionary is null, or the header name is empty.</exception>
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

        return DecodeJWTString(firstHeaderValue);
    }

    /// <summary>
    /// Decodes a JWT string from both Legacy and Cognito schemas and maps it to the Legacy Domain schema.
    /// </summary>
    /// <param name="jwtBase64Str">
    /// The base64 encoded user's JWT.
    /// If a header value of type (Microsoft.Extensions.Primitives.StringValues) that is pulled from the IHeaderDictionary is passed
    /// then an implicit conversion "StringValues.ToString()" will get triggered (only works with 1-valued headers)
    /// (see https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.primitives.stringvalues.tostring)
    /// </param>
    /// <returns>The deserialised Token or null</returns>
    public Token? DecodeJWTString(string jwtBase64Str)
    {
        try
        {
            var presentationToken = this.Decode<TokenPresentation>(jwtBase64Str);

            if (presentationToken is null)
            {
                _logger.LogWarning("Failed to deserialize JWT payload.");
                return null;
            }

            return MapToDomain(presentationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected, Null, or Malformed token: {InvalidToken}.", jwtBase64Str);
            return null;
        }
    }

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
}