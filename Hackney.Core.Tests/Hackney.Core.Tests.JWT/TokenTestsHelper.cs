using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using AutoFixture;
using Hackney.Core.JWT;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoFixture.Dsl;
using System;

namespace Hackney.Core.Tests.JWT
{
    internal static class TokenTestsHelper
    {
        public static string TestSecret => "this-is-a-very-long-test-secret-key-that-is-at-least-32-bytes!";
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

        public static TokenPresentation GenerateTestTokenObj(TokenSchema tokenSchema)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long oneHourFromNow = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();

            var builder = _fixture.Build<TokenPresentation>();

            IPostprocessComposer<TokenPresentation> composer = builder
                .With(t => t.Iat, now)
                .With(t => t.Nbf, now)
                .With(t => t.Exp, oneHourFromNow);

            if (tokenSchema == TokenSchema.Cognito)
            {
                composer = composer
                    .With(t => t.Groups, null as string[])
                    .With(t => t.CustomGroups, GenerateCognitoTokenGroupsString(count: 5));
            }
            else
            {
                composer = composer
                    .With(t => t.Groups, GenerateGoogleGroups(count: 5).ToArray())
                    .With(t => t.CustomGroups, null as string);
            }

            return composer.Create();
        }


        public static string GenerateCleanJwt(TokenPresentation token, string secret)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new Dictionary<string, object>
            {
                { "sub", token.Sub },
                { "email", token.Email },
                { "name", token.Name }
            };

            if (token.Groups != null)
            {
                claims.Add("groups", token.Groups);
            }
            else
            {
                claims.Add("custom:groups", token.CustomGroups);
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                SigningCredentials = credentials,

                // converts 'long' unix timestamps to date times
                Expires = DateTimeOffset.FromUnixTimeSeconds(token.Exp).UtcDateTime,
                NotBefore = DateTimeOffset.FromUnixTimeSeconds(token.Nbf).UtcDateTime,
                IssuedAt = DateTimeOffset.FromUnixTimeSeconds(token.Iat).UtcDateTime
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(descriptor);
            return handler.WriteToken(securityToken);
        }

        public static TestToken GenerateTestTokenPresentationJWT(TokenSchema tokenSchema)
        {
            var presentationToken = GenerateTestTokenObj(tokenSchema);
            return new TestToken
            {
                JwtString = GenerateCleanJwt(presentationToken, TestSecret),
                TokenObj = presentationToken
            };
        }
    }

    internal class TestToken
    {
        public string JwtString { get; set; }
        public TokenPresentation TokenObj { get; set; }

        public IEnumerable<string> GetCognitoTestUserGroups()
        {
            return !string.IsNullOrWhiteSpace(TokenObj?.CustomGroups) ?
                TokenObj.CustomGroups.Split(TokenTestsHelper.CognitoTokenGoogleGroupsSeparator).ToArray() :
                Array.Empty<string>();
        }

        public IEnumerable<string> GetLegacyTestUserGroups()
        {
            return TokenObj?.Groups ?? Array.Empty<string>();
        }
    }

    internal enum TokenSchema
    {
        Old,
        Cognito
    }
}
