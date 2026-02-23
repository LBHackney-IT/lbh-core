using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using AutoFixture;
using Hackney.Core.JWT;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hackney.Core.Tests.JWT
{
    internal static class TokenTestsHelper
    {
        public static char CognitoTokenGoogleGroupsSeparator => ';';
        private static Fixture _fixture = new Fixture();

        private static IEnumerable<string> GenerateGoogleGroups(int count)
        {
            if (count < 1) return Enumerable.Empty<string>();

            return Enumerable.Range(1, count).Select(i => $"Google-Group-{i}").ToList();
        }

        private static string GenerateCognitoTokenGroupsString(int count)
        {
            var googleGroups = GenerateGoogleGroups(count);

            return string.Join(CognitoTokenGoogleGroupsSeparator, googleGroups);
        }

        private static TokenPresentation GenerateTestTokenPresentationObj()
        {
            return _fixture.Build<TokenPresentation>()
                .With(x => x.CustomGroups, GenerateCognitoTokenGroupsString(count: 5))
                .Create();
        }

        private static Token GenerateTestTokenObj()
        {
            return _fixture.Build<Token>()
                .With(x => x.Groups, GenerateGoogleGroups(count: 5))
                .Create();
        }

        public static string GenerateCleanJwt(TokenPresentation token, string secret)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new Dictionary<string, object>
            {
                { "sub", token.Sub },
                { "email", token.Email },
                { "name", token.Name },
                { "nbf", token.Nbf },
                { "exp", token.Exp },
                { "iat", token.Iat }
            };

            // If old token schema
            if (token.Groups != null)
            {
                claims.Add("groups", token.Groups);
            }
            else // If cognito token
            {
                claims.Add("custom:groups", token.CustomGroups);
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                SigningCredentials = credentials
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(descriptor);
            return handler.WriteToken(securityToken);
        }
    }
}
