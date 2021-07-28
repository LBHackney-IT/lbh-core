using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Hackney.Core.JWT
{
    public class TokenFactory : ITokenFactory
    {
        public Token Create(IHeaderDictionary headerDictionary)
        {
            if (headerDictionary is null) throw new ArgumentNullException(nameof(headerDictionary));

            var encodedStringValueToken = headerDictionary["Authorization"];
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