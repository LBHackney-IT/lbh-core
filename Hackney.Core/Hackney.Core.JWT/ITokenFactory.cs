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
    /// Creates a domain <see cref="Token"/> by extracting a JWT from the specified header and decoding it.
    /// </summary>
    /// <param name="headerDictionary">Request headers.</param>
    /// <param name="headerName">Header key to read (default: <c>Authorization</c>).</param>
    /// <returns>
    /// The decoded <see cref="Token"/>, or <c>null</c> when the header is missing, empty,
    /// or decoding fails. Implementations should log decoding failures and not throw for bad tokens.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="headerDictionary"/> is null or <paramref name="headerName"/> is null or empty.</exception>
    [Obsolete("Deprecated to decouple token parsing from HTTP abstractions. This method will be removed entirely in the next major version. Extract the token string in the consuming API and use Decode instead.", error: false, DiagnosticId = "HACKNEY_DEPRECATED_TOKEN_CREATE")]
    Token? Create(IHeaderDictionary headerDictionary, string headerName = DefaultHeaderName);
    Token? DecodeStandardToken(string jwtStr);
    T? Decode<T>(string jwtStr) where T : class;
    HackneyTokenType IdentifyHackneyToken(string jwtStr);
}
