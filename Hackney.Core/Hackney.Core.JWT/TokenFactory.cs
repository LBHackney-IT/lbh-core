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
        // Separator aws cognito pre-token lambda uses to join Google group names with
        public char CognitoTokenGoogleGroupsSeparator => ';';

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

            return DecodeJWTString(encodedStringValueToken);
        }

        /// <summary>
        /// Decodes a JWT string from both Legacy and Cognito schemas and maps it to the Token schema.
        /// </summary>
        /// <param name="tokenHeaderValue">
        /// The base64 encoded user's JWT StringValues primitive
        /// </param>
        /// <returns>The deserialised Token or null</returns>
        /// <exception cref="System.ArgumentNullException">If the headerDictionary is null, or the header name is empty.</exception>
        public Token DecodeJWTString(Microsoft.Extensions.Primitives.StringValues tokenHeaderValue)
        {
            if (tokenHeaderValue.Count == 0)
                return null;

            var encodedString = tokenHeaderValue.ToArray().First().Replace("Bearer ", "", StringComparison.CurrentCultureIgnoreCase);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(encodedString);

            var decodedPayload = Base64UrlEncoder.Decode(jwtToken.EncodedPayload);

            // clean architecture principles - separate out presentation concern from domain logic 
            var presentationToken = JsonConvert.DeserializeObject<TokenPresentation>(decodedPayload);

            if (presentationToken == null)
                return null;

            // preserve the model that domain logic expects
            return MapToDomain(presentationToken);
        }

        private Token MapToDomain(TokenPresentation presentation)
        {
            string[] parsedGroups = Array.Empty<string>();

            if (presentation.Groups != null)
            {
                parsedGroups = presentation.Groups;
            }
            else if (!string.IsNullOrWhiteSpace(presentation.CustomGroups))
            {
                parsedGroups = presentation.CustomGroups.Split(this.CognitoTokenGoogleGroupsSeparator, StringSplitOptions.RemoveEmptyEntries);
            }

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
}