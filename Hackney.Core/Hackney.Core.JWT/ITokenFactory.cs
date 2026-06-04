using System;
using Microsoft.AspNetCore.Http;

namespace Hackney.Core.JWT;

/// <summary>
/// Interface defining the creation of a token object from a JWT included in Http headers
/// </summary>
public interface ITokenFactory
{
    /// <summary>
    /// The default Http header name used to create a token
    /// </summary>
    public const string DefaultHeaderName = "Authorization";

    /// <summary>
    /// Extracts a JWT from the supplied Http headers and creates a token object from it.
    /// </summary>
    /// <param name="headerDictionary">The Http headers</param>
    /// <param name="headerName">The header key name used. Default: "Authorization"</param>
    /// <returns>The deserialised Token or null</returns>
    /// <exception cref="System.ArgumentNullException">If the headerDictionary is null, or the header name is empty.</exception>
    [Obsolete("Deprecated to decouple token parsing from HTTP abstractions. This method will be removed entirely in the next major version. Extract the token string in the consuming API and use Decode instead.", error: false, DiagnosticId = "HACKNEY_DEPRECATED_TOKEN_CREATE")]
    Token? Create(IHeaderDictionary headerDictionary, string headerName = DefaultHeaderName);
    Token? DecodeStandardToken(string jwtStr);
    T? Decode<T>(string jwtStr) where T : class;
    HackneyTokenType IdentifyHackneyToken(string jwtStr);
}
