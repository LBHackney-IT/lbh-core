using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Hackney.Core.JWT
{
    /// <summary>
    /// Class implementing the creation of a token object from a JWT included in Http headers
    /// </summary>
    public class TokenFactory : ITokenFactory
    {
        /// <summary>
        /// Extracts a JWT from the supplied Http headers and creates a token object from it.
        /// </summary>
        /// <param name="headerDictionary">The Http headers</param>
        /// <param name="headerName">The header key name used. Default: "Authorization"</param>
        /// <returns>The deserialised Token or null</returns>
        /// <exception cref="System.ArgumentNullException">If the headerDictionary is null, or the header name is empty.</exception>
        public Token Create(IHeaderDictionary headerDictionary, string headerName = ITokenFactory.DefaultHeaderName)
        {
            if (headerDictionary is null) throw new ArgumentNullException(nameof(headerDictionary));
            if (string.IsNullOrEmpty(headerName)) throw new ArgumentNullException(headerName);

            var encodedStringValueToken = headerDictionary[headerName];
            if (encodedStringValueToken.Count == 0)
                return null;

            var encodedString = encodedStringValueToken.ToArray().First().Replace("Bearer ", "", StringComparison.CurrentCultureIgnoreCase);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(encodedString);
            var decodedPayload = Base64UrlEncoder.Decode(jwtToken.EncodedPayload);
            return JsonConvert.DeserializeObject<Token>(decodedPayload);
        }
    }
}