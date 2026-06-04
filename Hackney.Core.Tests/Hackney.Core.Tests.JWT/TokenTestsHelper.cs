using System.Collections.Generic;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Linq;
using AutoFixture;
using Hackney.Core.JWT;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using AutoFixture.Dsl;
using System;
using System.Security.Cryptography;

namespace Hackney.Core.Tests.JWT;

internal static class TokenTestsHelper
{
    public static string TestSecret => "this-is-a-very-long-test-secret-key-that-is-at-least-32-bytes!";
    public static char CognitoTokenGoogleGroupsSeparator => ';';
    private static readonly Fixture _fixture = new();

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
        else if (token.CustomGroups != null)
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

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }

    public static TestToken GenerateTestTokenPresentationJWT(TokenSchema tokenSchema)
    {
        var presentationToken = GenerateTestTokenObj(tokenSchema);
        return new TestToken(
            jwtString: GenerateCleanJwt(presentationToken, TestSecret),
            tokenObject: presentationToken
        );
    }

    public static string GenerateBasicGeneralPayloadToken(object? payloadData, bool isSigned = false)
    {
        var header = new Dictionary<string, object>
        {
            ["alg"] = isSigned ? SecurityAlgorithms.HmacSha256 : SecurityAlgorithms.None,
            ["typ"] = "JWT"
        };

        var headerJson = JsonSerializer.Serialize(header);
        var encodedHeader = Base64UrlEncoder.Encode(headerJson);

        var payloadJson = JsonSerializer.Serialize(payloadData);
        var encodedPayload = Base64UrlEncoder.Encode(payloadJson);

        var signingInput = encodedHeader + "." + encodedPayload;

        if (!isSigned)
        {
            // unsigned token: include empty signature segment for compatibility
            return signingInput + ".";
        }

        // signed token: compute HMACSHA256 over signingInput using TestSecret
        var keyBytes = Encoding.UTF8.GetBytes(TestSecret);
        using var hmac = new HMACSHA256(keyBytes);
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
        var signature = Base64UrlEncoder.Encode(signatureBytes);

        return signingInput + "." + signature;
    }

    public static string GenerateTokenWithNullPayload() => GenerateBasicGeneralPayloadToken(null);

    public static string GenerateTokenWithRawStringPayload(string payloadString) => GenerateBasicGeneralPayloadToken(payloadString);

    public static string GenerateTokenWithEmptyObjectPayload() => GenerateBasicGeneralPayloadToken(new { });

    public static LegacyM2mTokenPayload GenerateLegacyM2mTokenObj()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return new LegacyM2mTokenPayload
        {
            Id = Guid.NewGuid().ToString(),
            ConsumerName = "TestConsumer",
            ConsumerType = "test",
            Nbf = now,
            Exp = now + 3600,
            Iat = now
        };
    }

    public static string GenerateLegacyM2mJwt(LegacyM2mTokenPayload payload) => GenerateBasicGeneralPayloadToken(payload, isSigned: true);

    public static string GenerateUnknownTokenWithPayload(object payload) => GenerateBasicGeneralPayloadToken(payload, isSigned: false);
}

internal class TestToken
{
    public string JwtString { get; set; }
    public TokenPresentation TokenObj { get; set; }

    public TestToken(string jwtString, TokenPresentation tokenObject)
    {
        JwtString = jwtString;
        TokenObj = tokenObject;
    }

    public IEnumerable<string> GetCognitoTestUserGroups()
    {
        return !string.IsNullOrWhiteSpace(TokenObj?.CustomGroups) ?
            TokenObj.CustomGroups.Split(TokenTestsHelper.CognitoTokenGoogleGroupsSeparator).ToArray() :
            Array.Empty<string>();
    }

    public IEnumerable<string> GetLegacyTestUserGroups()
    {
        return TokenObj.Groups ?? Array.Empty<string>();
    }
}

internal enum TokenSchema
{
    Old,
    Cognito
}

