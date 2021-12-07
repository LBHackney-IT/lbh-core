using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Hackney.Core.Testing.PactBroker
{
    /// <summary>
    /// Static class used to create a token that will be added to pact broker calls to the api
    /// in order to satisfy the api token expectations.
    /// TODO - Currently creates only a very basic token and need to be extended to cover different use cases.
    /// </summary>
    public static class TokenGenerator
    {
        public static string Generate(string audience)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "WebApiUser"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.Role, "PowerUser"),
            };
            var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "https://id.example.org",
                Audience = audience,
                Subject = identity,
                Expires = DateTime.UtcNow.AddHours(1)
                //,SigningCredentials = new SigningCredentials(Startup.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
