using System.IdentityModel.Tokens.Jwt;
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
            var encodedStringToken = headerDictionary["Authorization"];

            var jwtToken = handler.ReadJwtToken(encodedStringToken);
            var decodedPayload = Base64UrlEncoder.Decode(jwtToken.EncodedPayload);
            return JsonConvert.DeserializeObject<Token>(decodedPayload);
        }
    }
}