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
            var handler = new JwtSecurityTokenHandler();
            var encodedStringValueToken = headerDictionary["Authorization"];
            var encodedString = encodedStringValueToken.ToArray().First().Replace("Bearer ", "", StringComparison.CurrentCultureIgnoreCase);

            var jwtToken = handler.ReadJwtToken(encodedString);
            var decodedPayload = Base64UrlEncoder.Decode(jwtToken.EncodedPayload);
            return JsonConvert.DeserializeObject<Token>(decodedPayload);
        }
    }
}