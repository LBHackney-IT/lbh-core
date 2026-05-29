using FluentAssertions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using Xunit;

namespace Hackney.Core.Tests.JWT;

public class TokenFactoryTests
{
    private readonly Mock<IHeaderDictionary> _mockHeaders;
    private readonly Mock<ILogger<TokenFactory>> _mockLogger;
    private readonly TokenFactory _sut;

    public TokenFactoryTests()
    {
        var nonEmptyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        _mockHeaders = new Mock<IHeaderDictionary>();
        _mockHeaders.Setup(x => x["Authorization"]).Returns(nonEmptyToken.JwtString);

        _mockLogger = new Mock<ILogger<TokenFactory>>();

        _sut = new TokenFactory(_mockLogger.Object);
    }

    [Fact]
    public void TokenFactory_DecodeJWTString_LogsWarning_WhenNoJwtProvided()
    {
        // act
        _sut.DecodeJWTString("");

        // assert
        VerifyLog(_mockLogger, LogLevel.Warning, "No JWT token was provided.", Times.Once());
    }

    [Fact]
    public void TokenFactory_DecodeJWTString_LogsWarning_WhenPayloadIsEmptyObject()
    {
        // arrange
        var tokenWithEmptyObject = TokenTestsHelper.GenerateTokenWithEmptyObjectPayload();

        // act
        var result = _sut.DecodeJWTString(tokenWithEmptyObject);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, "JWT payload is empty JSON object.", Times.Once());
    }

    [Fact]
    public void TokenFactory_DecodeJWTString_LogsWarning_OnMalformedToken()
    {
        // arrange
        var invalidToken = "invalid-token-value";

        // act
        var result = _sut.DecodeJWTString(invalidToken);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, "Unexpected, Null, or Malformed token:", Times.Once());
    }

    [Fact]
    public void TokenFactory_DecodeJWTString_LogsWarning_WhenDeserialisationProducesNull()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithNullPayload();

        // act
        var result = _sut.DecodeJWTString(tokenWithNullPayload);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, "Unexpected, Null, or Malformed token:", Times.Once());
    }

    [Fact]
    public void TokenFactory_CreateMethod_Throws_GivenNullHeaders()
    {
        Action act = () => _sut.Create(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TokenFactory_CreateMethod_Throws_GivenEmptyHeaderName()
    {
        Action act = () => _sut.Create(_mockHeaders.Object, "");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TokenFactory_CreateMethod_ReturnsNull_GivenNoAuthorizationHeader()
    {
        _mockHeaders.Reset();

        _sut.Create(_mockHeaders.Object).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("some-header")]
    public void TokenFactory_CreateMethod_MapsLegacyTokenCorrectly_AndDefaultsToCorrectHeaderNameWhenItIsMissing(string headerName)
    {
        // arrange
        var testToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var expectedLegacyTokenGroups = testToken.GetLegacyTestUserGroups();
        var actualHeader = headerName ?? ITokenFactory.DefaultHeaderName;

        _mockHeaders.Reset();
        _mockHeaders.Setup(x => x[actualHeader]).Returns(testToken.JwtString);

        // act 
        var decodedToken = _sut.Create(_mockHeaders.Object, actualHeader);

        // assert
        decodedToken.Email.Should().Be(testToken.TokenObj?.Email);
        decodedToken.Exp.Should().Be(testToken.TokenObj?.Exp);
        decodedToken.Groups.Should().BeEquivalentTo(expectedLegacyTokenGroups);
        decodedToken.Iat.Should().Be(testToken.TokenObj?.Iat);
        decodedToken.Name.Should().Be(testToken.TokenObj?.Name);
        decodedToken.Nbf.Should().Be(testToken.TokenObj?.Nbf);
        decodedToken.Sub.Should().Be(testToken.TokenObj?.Sub);
    }

    [Fact]
    public void TokenFactory_CreateMethod_MapsTheCognitoTokenCorrectly()
    {
        // arrange
        var headerName = "Authorization";
        var testToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);
        var expectedGroupsArray = testToken.GetCognitoTestUserGroups();

        _mockHeaders.Reset();
        _mockHeaders.Setup(x => x[headerName]).Returns(testToken.JwtString);

        // act 
        var decodedToken = _sut.Create(headerDictionary: _mockHeaders.Object, headerName);

        // assert
        decodedToken.Email.Should().Be(testToken.TokenObj?.Email);
        decodedToken.Exp.Should().Be(testToken.TokenObj?.Exp);
        decodedToken.Groups.Should().BeEquivalentTo(expectedGroupsArray);
        decodedToken.Iat.Should().Be(testToken.TokenObj?.Iat);
        decodedToken.Name.Should().Be(testToken.TokenObj?.Name);
        decodedToken.Nbf.Should().Be(testToken.TokenObj?.Nbf);
        decodedToken.Sub.Should().Be(testToken.TokenObj?.Sub);
    }

    [Fact]
    public void TokenFactory_CreateMethod_ReturnsEmptyGroupsArray_WhenNeitherGroupsNorCustomGroupsIsSet()
    {
        // arrange
        var headerName = "Authorization";
        var schemaIrrelForTest = TokenSchema.Cognito;

        var grouplessToken = TokenTestsHelper.GenerateTestTokenObj(schemaIrrelForTest);
        grouplessToken.Groups = null;
        grouplessToken.CustomGroups = null;

        var grouplessTokenJwtString = TokenTestsHelper.GenerateCleanJwt(grouplessToken, TokenTestsHelper.TestSecret);

        _mockHeaders.Reset();
        _mockHeaders.Setup(x => x[headerName]).Returns(grouplessTokenJwtString);

        // act 
        var decodedToken = _sut.Create(headerDictionary: _mockHeaders.Object, headerName);

        // assert
        // parses fields that exist
        decodedToken.Email.Should().Be(grouplessToken.Email);
        decodedToken.Exp.Should().Be(grouplessToken.Exp);
        decodedToken.Iat.Should().Be(grouplessToken.Iat);
        decodedToken.Name.Should().Be(grouplessToken.Name);
        decodedToken.Nbf.Should().Be(grouplessToken.Nbf);
        decodedToken.Sub.Should().Be(grouplessToken.Sub);
        // defaults to empty array when no groups are found
        decodedToken.Groups.Should().BeEquivalentTo(Array.Empty<string>());
    }

    [Fact]
    public void TokenFactory_DecodeJWTStringAndCreateMethods_CanSafelyHandleBearerPrefix()
    {
        // arrange
        var legacyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var cognitoToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        var bearerPrefix = "Bearer ";
        legacyToken.JwtString = bearerPrefix + legacyToken.JwtString;
        cognitoToken.JwtString = bearerPrefix + cognitoToken.JwtString;

        // act 
        var decodedLegacyToken = _sut.DecodeJWTString(legacyToken.JwtString);
        var decodedCognitoToken = _sut.DecodeJWTString(cognitoToken.JwtString);

        // assert
        decodedLegacyToken.Should().NotBeNull();
        decodedCognitoToken.Should().NotBeNull();

        // only asserting a few fields as if even 1 field was decoded, it means
        // that the JWT parser didn't fall over by creating an empty object with no data.
        decodedLegacyToken.Email.Should().Be(legacyToken.TokenObj?.Email);
        decodedCognitoToken.Email.Should().Be(cognitoToken.TokenObj?.Email);
    }

    [Fact]
    public void TokenFactory_DecodeJWTStringAndCreateMethods_ReturnsNull_GivenTokenWithLiteralNullPayload()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithNullPayload();

        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(tokenWithNullPayload);

        // act
        var decodedNullTokenViaCreate = _sut.Create(_mockHeaders.Object);
        var decodedNullTokenViaDecode = _sut.DecodeJWTString(tokenWithNullPayload);

        // assert
        decodedNullTokenViaCreate.Should().BeNull();
        decodedNullTokenViaDecode.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeJWTStringAndCreateMethods_ReturnsNull_GivenTokenWithEmptyObjectPayload()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithEmptyObjectPayload();

        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(tokenWithNullPayload);

        // act
        var decodedNullTokenViaCreate = _sut.Create(_mockHeaders.Object);
        var decodedNullTokenViaDecode = _sut.DecodeJWTString(tokenWithNullPayload);

        // assert
        decodedNullTokenViaCreate.Should().BeNull();
        decodedNullTokenViaDecode.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeJWTStringAndCreateMethods_ReturnsNull_GivenTokenWithRawStringPayload()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithRawStringPayload("Sheep Detectives 2026");

        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(tokenWithNullPayload);

        // act
        var decodedNullTokenViaCreate = _sut.Create(_mockHeaders.Object);
        var decodedNullTokenViaDecode = _sut.DecodeJWTString(tokenWithNullPayload);

        // assert
        decodedNullTokenViaCreate.Should().BeNull();
        decodedNullTokenViaDecode.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeJWTStringAndCreateMethods_AreCapableOfHandlingStingValuesPrimitivesFromHeaderDict()
    {
        // arrange
        var cognitoToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        var headerName = "Authorization";
        var headersDict = new HeaderDictionary
        {
            { headerName, cognitoToken.JwtString }
        };

        StringValues strValPrimitiveHeaderVal = headersDict[headerName];

        // act 
        var decodedTokenViaCreate = _sut.Create(headersDict, headerName);
        var decodedTokenViaDecode = _sut.DecodeJWTString(strValPrimitiveHeaderVal);

        // assert
        decodedTokenViaCreate.Should().NotBeNull();
        decodedTokenViaDecode.Should().NotBeNull();

        // only asserting a few fields as if even 1 field was decoded, it means
        // that the JWT parser didn't fall over by creating an empty object with no data.
        var expectedEmail = cognitoToken.TokenObj?.Email;
        decodedTokenViaCreate.Email.Should().Be(expectedEmail);
        decodedTokenViaDecode.Email.Should().Be(expectedEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bearer ")]
    [InlineData("invalid-token-value")]
    [InlineData("    ")]
    [InlineData("Bearer   ")]
    [InlineData("Bearer [object Object]")]
    public void TokenFactory_DecodeJWTStringAndCreateMethods_ReturnNull_GivenInvalidJWTInput(string invalidJwtString)
    {
        // arrange
        var invalidToken = new TestToken(invalidJwtString, null);

        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(invalidJwtString);

        // act
        var decodeMethodResult = _sut.DecodeJWTString(invalidJwtString);
        var createMethodResult = _sut.Create(_mockHeaders.Object);

        // assert
        decodeMethodResult.Should().BeNull();
        createMethodResult.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeJWTStringMethod_CanDecodeTokenIndependentOfHeaders_GivenTheRawBase64StringIsProvided()
    {
        // arrange
        var legacyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var cognitoToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        // act 
        var decodedLegacyToken = _sut.DecodeJWTString(legacyToken.JwtString);
        var decodedCognitoToken = _sut.DecodeJWTString(cognitoToken.JwtString);

        // assert
        decodedLegacyToken.Email.Should().Be(legacyToken.TokenObj?.Email);
        decodedLegacyToken.Exp.Should().Be(legacyToken.TokenObj?.Exp);
        decodedLegacyToken.Iat.Should().Be(legacyToken.TokenObj?.Iat);
        decodedLegacyToken.Name.Should().Be(legacyToken.TokenObj?.Name);
        decodedLegacyToken.Nbf.Should().Be(legacyToken.TokenObj?.Nbf);
        decodedLegacyToken.Sub.Should().Be(legacyToken.TokenObj?.Sub);
        decodedLegacyToken.Groups.Should().BeEquivalentTo(legacyToken.GetLegacyTestUserGroups());

        decodedCognitoToken.Email.Should().Be(cognitoToken.TokenObj?.Email);
        decodedCognitoToken.Exp.Should().Be(cognitoToken.TokenObj?.Exp);
        decodedCognitoToken.Iat.Should().Be(cognitoToken.TokenObj?.Iat);
        decodedCognitoToken.Name.Should().Be(cognitoToken.TokenObj?.Name);
        decodedCognitoToken.Nbf.Should().Be(cognitoToken.TokenObj?.Nbf);
        decodedCognitoToken.Sub.Should().Be(cognitoToken.TokenObj?.Sub);
        decodedCognitoToken.Groups.Should().BeEquivalentTo(cognitoToken.GetCognitoTestUserGroups());
    }

    private static void VerifyLog(Mock<ILogger<TokenFactory>> mockLogger, LogLevel level, string expectedMessage, Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times);
    }
}
