using FluentAssertions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using Xunit;

#pragma warning disable HACKNEY_DEPRECATED_TOKEN_CREATE

namespace Hackney.Core.Tests.JWT;

public class TokenFactoryTests
{
    private readonly Mock<IHeaderDictionary> _mockHeaders;
    private readonly Mock<ILogger<TokenFactory>> _mockLogger;
    private readonly TokenFactory _sut;
    private readonly int LoggedTokenTruncateLimit = 30;

    public TokenFactoryTests()
    {
        var nonEmptyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        _mockHeaders = new Mock<IHeaderDictionary>();
        _mockHeaders.Setup(x => x["Authorization"]).Returns(nonEmptyToken.JwtString);

        _mockLogger = new Mock<ILogger<TokenFactory>>();

        _sut = new TokenFactory(_mockLogger.Object);
    }

    [Fact]
    public void TokenFactory_DecodeStandardToken_LogsWarning_WhenNoJwtProvided()
    {
        // act
        _sut.DecodeStandardToken("");

        // assert
        VerifyLog(_mockLogger, LogLevel.Warning, "No JWT token was provided.", Times.Once());
    }

    [Fact]
    public void TokenFactory_DecodeStandardToken_LogsWarning_WhenPayloadIsEmptyObject()
    {
        // arrange
        var tokenWithEmptyObject = TokenTestsHelper.GenerateTokenWithEmptyObjectPayload();

        // act
        var result = _sut.DecodeStandardToken(tokenWithEmptyObject);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, "JWT payload is empty JSON object.", Times.Once());
    }

    [Fact]
    public void TokenFactory_DecodeStandardToken_LogsWarning_OnMalformedToken()
    {
        // arrange
        var invalidToken = "invalid-token-value";
        var expectedToken = invalidToken[..Math.Min(invalidToken.Length, LoggedTokenTruncateLimit)];

        // act
        var result = _sut.DecodeStandardToken(invalidToken);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, $"Unexpected, Null, or Malformed token: {expectedToken}", Times.Once());
    }

    [Fact]
    public void TokenFactory_DecodeStandardToken_LogsWarning_WhenDeserialisationProducesNull()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithNullPayload();
        var expectedToken = tokenWithNullPayload[..Math.Min(tokenWithNullPayload.Length, LoggedTokenTruncateLimit)];

        // act
        var result = _sut.DecodeStandardToken(tokenWithNullPayload);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, $"Unexpected, Null, or Malformed token: {expectedToken}", Times.Once());
    }

    [Fact]
    public void TokenFactory_DecodeStandardToken_LogsWarning_WhenDeserialisationProducesNull_WhileSanitizingAndTruncatingToken()
    {
        // arrange
        var tokenWithNullPayload = new string[]
        {
            "fakeToken",
            "[Info] User barbastella.barbastellus@moccas-hill-wood.co.uk has been granted access.",
            "[Info] Consult an ecologist before doing any development or maintenance on this API."
        };

        var fullTokenPayload = string.Join("\r\n", tokenWithNullPayload);
        var expectedToken = string.Join(string.Empty, tokenWithNullPayload)[..LoggedTokenTruncateLimit];

        // act
        var result = _sut.DecodeStandardToken(fullTokenPayload);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, $"Unexpected, Null, or Malformed token: {expectedToken}", Times.Once());
    }

    [Fact]
    public void TokenFactory_CreateMethod_Throws_GivenNullHeaders()
    {
        Action act = () => _sut.Create(null!);
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

    [Fact]
    public void TokenFactory_CreateMethod_LogsWarning_WhenHeaderIsEmpty()
    {
        // arrange
        var headerName = "Authorization";

        _mockHeaders.Reset();
        _mockHeaders.Setup(x => x[headerName]).Returns(StringValues.Empty);

        // act
        var result = _sut.Create(_mockHeaders.Object, headerName);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, $"Provided header '{headerName}' is empty.", Times.Once());
    }

    [Fact]
    public void TokenFactory_CreateMethod_LogsWarning_WhenFirstHeaderValueIsEmpty()
    {
        // arrange
        var headerName = "Authorization";

        _mockHeaders.Reset();
        _mockHeaders.Setup(x => x[headerName]).Returns(new StringValues(new[] { string.Empty }));

        // act
        var result = _sut.Create(_mockHeaders.Object, headerName);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, "First extracted HeaderDictionary StringValues primitive value is empty.", Times.Once());
    }

    [Fact]
    public void TokenFactory_CreateMethod_LogsWarning_WhenFirstHeaderValueIsNull()
    {
        // arrange
        var headerName = "Authorization";

        _mockHeaders.Reset();
        _mockHeaders.Setup(x => x[headerName]).Returns(new StringValues(new[] { (string?)null }));

        // act
        var result = _sut.Create(_mockHeaders.Object, headerName);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, "First extracted HeaderDictionary StringValues primitive value is empty.", Times.Once());
    }

    [Fact]
    public void TokenFactory_CreateMethod_UsesFirstHeaderValueAndLogsWarning_WhenMultipleHeaderValuesProvided()
    {
        // arrange
        var headerName = "Authorization";
        var firstToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);
        var secondToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        var multiValHeaderValues = new StringValues(new[] { firstToken.JwtString, secondToken.JwtString });

        _mockHeaders.Reset();
        _mockHeaders.Setup(x => x[headerName]).Returns(multiValHeaderValues);

        // act
        var decoded = _sut.Create(_mockHeaders.Object, headerName);

        // assert
        decoded.Should().NotBeNull();
        decoded!.Sub.Should().Be(firstToken.TokenObj.Sub);
        VerifyLog(_mockLogger, LogLevel.Warning, $"Multiple (count: {multiValHeaderValues.Count}) header values detected, using the first one.", Times.Once());
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
        decodedToken!.Email.Should().Be(testToken.TokenObj.Email);
        decodedToken!.Exp.Should().Be(testToken.TokenObj.Exp);
        decodedToken!.Groups.Should().BeEquivalentTo(expectedLegacyTokenGroups);
        decodedToken!.Iat.Should().Be(testToken.TokenObj.Iat);
        decodedToken!.Name.Should().Be(testToken.TokenObj.Name);
        decodedToken!.Nbf.Should().Be(testToken.TokenObj.Nbf);
        decodedToken!.Sub.Should().Be(testToken.TokenObj.Sub);
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
        decodedToken!.Email.Should().Be(testToken.TokenObj.Email);
        decodedToken!.Exp.Should().Be(testToken.TokenObj.Exp);
        decodedToken!.Groups.Should().BeEquivalentTo(expectedGroupsArray);
        decodedToken!.Iat.Should().Be(testToken.TokenObj.Iat);
        decodedToken!.Name.Should().Be(testToken.TokenObj.Name);
        decodedToken!.Nbf.Should().Be(testToken.TokenObj.Nbf);
        decodedToken!.Sub.Should().Be(testToken.TokenObj.Sub);
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
        decodedToken!.Email.Should().Be(grouplessToken.Email);
        decodedToken!.Exp.Should().Be(grouplessToken.Exp);
        decodedToken!.Iat.Should().Be(grouplessToken.Iat);
        decodedToken!.Name.Should().Be(grouplessToken.Name);
        decodedToken!.Nbf.Should().Be(grouplessToken.Nbf);
        decodedToken!.Sub.Should().Be(grouplessToken.Sub);
        // defaults to empty array when no groups are found
        decodedToken!.Groups.Should().BeEquivalentTo(Array.Empty<string>());
    }

    [Fact]
    public void TokenFactory_DecodeStandardTokenAndCreateMethods_CanSafelyHandleBearerPrefix()
    {
        // arrange
        var legacyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var cognitoToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        var bearerPrefix = "Bearer ";
        legacyToken.JwtString = bearerPrefix + legacyToken.JwtString;
        cognitoToken.JwtString = bearerPrefix + cognitoToken.JwtString;

        // act 
        var decodedLegacyToken = _sut.DecodeStandardToken(legacyToken.JwtString!);
        var decodedCognitoToken = _sut.DecodeStandardToken(cognitoToken.JwtString!);

        // assert
        decodedLegacyToken.Should().NotBeNull();
        decodedCognitoToken.Should().NotBeNull();

        // only asserting a few fields as if even 1 field was decoded, it means
        // that the JWT parser didn't fall over by creating an empty object with no data.
        decodedLegacyToken!.Email.Should().Be(legacyToken.TokenObj.Email);
        decodedCognitoToken!.Email.Should().Be(cognitoToken.TokenObj.Email);
    }

    [Fact]
    public void TokenFactory_DecodeStandardTokenAndCreateMethods_ReturnsNull_GivenTokenWithLiteralNullPayload()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithNullPayload();

        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(tokenWithNullPayload);

        // act
        var decodedNullTokenViaCreate = _sut.Create(_mockHeaders.Object);
        var decodedNullTokenViaDecode = _sut.DecodeStandardToken(tokenWithNullPayload);

        // assert
        decodedNullTokenViaCreate.Should().BeNull();
        decodedNullTokenViaDecode.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeStandardTokenAndCreateMethods_ReturnsNull_GivenTokenWithEmptyObjectPayload()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithEmptyObjectPayload();

        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(tokenWithNullPayload);

        // act
        var decodedNullTokenViaCreate = _sut.Create(_mockHeaders.Object);
        var decodedNullTokenViaDecode = _sut.DecodeStandardToken(tokenWithNullPayload);

        // assert
        decodedNullTokenViaCreate.Should().BeNull();
        decodedNullTokenViaDecode.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeStandardTokenAndCreateMethods_ReturnsNull_GivenTokenWithRawStringPayload()
    {
        // arrange
        var tokenWithNullPayload = TokenTestsHelper.GenerateTokenWithRawStringPayload("Sheep Detectives 2026");

        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(tokenWithNullPayload);

        // act
        var decodedNullTokenViaCreate = _sut.Create(_mockHeaders.Object);
        var decodedNullTokenViaDecode = _sut.DecodeStandardToken(tokenWithNullPayload);

        // assert
        decodedNullTokenViaCreate.Should().BeNull();
        decodedNullTokenViaDecode.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeStandardTokenAndCreateMethods_AreCapableOfHandlingStingValuesPrimitivesFromHeaderDict()
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
        var decodedTokenViaDecode = _sut.DecodeStandardToken(strValPrimitiveHeaderVal!);

        // assert
        decodedTokenViaCreate.Should().NotBeNull();
        decodedTokenViaDecode.Should().NotBeNull();

        // only asserting a few fields as if even 1 field was decoded, it means
        // that the JWT parser didn't fall over by creating an empty object with no data.
        var expectedEmail = cognitoToken.TokenObj.Email;
        decodedTokenViaCreate!.Email.Should().Be(expectedEmail);
        decodedTokenViaDecode!.Email.Should().Be(expectedEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bearer ")]
    [InlineData("invalid-token-value")]
    [InlineData("    ")]
    [InlineData("Bearer   ")]
    [InlineData("Bearer [object Object]")]
    public void TokenFactory_DecodeStandardTokenAndCreateMethods_ReturnNull_GivenInvalidJWTInput(string invalidJwtString)
    {
        // arrange
        _mockHeaders.Reset();
        _mockHeaders.Setup(h => h[It.IsAny<string>()]).Returns(invalidJwtString);

        // act
        var decodeMethodResult = _sut.DecodeStandardToken(invalidJwtString);
        var createMethodResult = _sut.Create(_mockHeaders.Object);

        // assert
        decodeMethodResult.Should().BeNull();
        createMethodResult.Should().BeNull();
    }

    [Fact]
    public void TokenFactory_DecodeStandardTokenMethod_CanDecodeTokenIndependentOfHeaders_GivenTheRawBase64String()
    {
        // arrange
        var legacyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var cognitoToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        // act 
        var decodedLegacyToken = _sut.DecodeStandardToken(legacyToken.JwtString);
        var decodedCognitoToken = _sut.DecodeStandardToken(cognitoToken.JwtString);

        // assert
        decodedLegacyToken.Should().NotBeNull();

        decodedLegacyToken!.Email.Should().Be(legacyToken.TokenObj.Email);
        decodedLegacyToken!.Exp.Should().Be(legacyToken.TokenObj.Exp);
        decodedLegacyToken!.Iat.Should().Be(legacyToken.TokenObj.Iat);
        decodedLegacyToken!.Name.Should().Be(legacyToken.TokenObj.Name);
        decodedLegacyToken!.Nbf.Should().Be(legacyToken.TokenObj.Nbf);
        decodedLegacyToken!.Sub.Should().Be(legacyToken.TokenObj.Sub);
        decodedLegacyToken!.Groups.Should().BeEquivalentTo(legacyToken.GetLegacyTestUserGroups());

        decodedCognitoToken.Should().NotBeNull();

        decodedCognitoToken!.Email.Should().Be(cognitoToken.TokenObj.Email);
        decodedCognitoToken!.Exp.Should().Be(cognitoToken.TokenObj.Exp);
        decodedCognitoToken!.Iat.Should().Be(cognitoToken.TokenObj.Iat);
        decodedCognitoToken!.Name.Should().Be(cognitoToken.TokenObj.Name);
        decodedCognitoToken!.Nbf.Should().Be(cognitoToken.TokenObj.Nbf);
        decodedCognitoToken!.Sub.Should().Be(cognitoToken.TokenObj.Sub);
        decodedCognitoToken!.Groups.Should().BeEquivalentTo(cognitoToken.GetCognitoTestUserGroups());
    }

    [Fact]
    public void TokenFactory_GenericDecode_CanDecodeToken_GivenTheRawJwtBase64String()
    {
        // arrange
        var legacyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var cognitoToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        // act 
        var decodedLegacy = _sut.Decode<TokenPresentation>(legacyToken.JwtString);
        var decodedCognito = _sut.Decode<TokenPresentation>(cognitoToken.JwtString);

        // assert
        decodedLegacy.Should().NotBeNull();
        decodedLegacy!.Email.Should().Be(legacyToken.TokenObj.Email);
        decodedLegacy!.Exp.Should().Be(legacyToken.TokenObj.Exp);
        decodedLegacy!.Iat.Should().Be(legacyToken.TokenObj.Iat);
        decodedLegacy!.Name.Should().Be(legacyToken.TokenObj.Name);
        decodedLegacy!.Nbf.Should().Be(legacyToken.TokenObj.Nbf);
        decodedLegacy!.Sub.Should().Be(legacyToken.TokenObj.Sub);
        decodedLegacy!.Groups.Should().BeEquivalentTo(legacyToken.GetLegacyTestUserGroups());
        decodedLegacy!.CustomGroups.Should().BeEquivalentTo(null);

        decodedCognito.Should().NotBeNull();
        decodedCognito!.Email.Should().Be(cognitoToken.TokenObj.Email);
        decodedCognito!.Exp.Should().Be(cognitoToken.TokenObj.Exp);
        decodedCognito!.Iat.Should().Be(cognitoToken.TokenObj.Iat);
        decodedCognito!.Name.Should().Be(cognitoToken.TokenObj.Name);
        decodedCognito!.Nbf.Should().Be(cognitoToken.TokenObj.Nbf);
        decodedCognito!.Sub.Should().Be(cognitoToken.TokenObj.Sub);
        decodedCognito!.Groups.Should().BeEquivalentTo(null);
        decodedCognito!.CustomGroups.Should().BeEquivalentTo(string.Join(';', cognitoToken.GetCognitoTestUserGroups()));
    }

    [Fact]
    public void TokenFactory_Decode_HandlesBearerPrefix()
    {
        // arrange
        var legacyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var bearer = "Bearer ";

        // act
        var decoded = _sut.Decode<TokenPresentation>(bearer + legacyToken.JwtString);

        // assert
        decoded.Should().NotBeNull();
        decoded!.Email.Should().Be(legacyToken.TokenObj.Email);
    }

    [Fact]
    public void TokenFactory_Decode_LogsWarning_WhenNoJwtProvided_Generic()
    {
        // act
        var result = _sut.Decode<TokenPresentation>("");

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, "No JWT token was provided.", Times.Once());
    }

    [Fact]
    public void TokenFactory_Decode_LogsWarning_OnMalformedToken_Generic()
    {
        // arrange
        var invalidToken = "invalid-token-value";

        // act
        var result = _sut.Decode<TokenPresentation>(invalidToken);

        // assert
        result.Should().BeNull();
        VerifyLog(_mockLogger, LogLevel.Warning, $"Unexpected, Null, or Malformed token: {invalidToken}.", Times.Once());
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_ReturnsMachineLegacyType_ForLegacyM2mPayload()
    {
        // arrange
        var m2mPayload = TokenTestsHelper.GenerateLegacyM2mTokenObj();
        var m2mJwtStr = TokenTestsHelper.GenerateLegacyM2mJwt(m2mPayload);

        // act
        var resultTokenType = _sut.IdentifyHackneyToken(m2mJwtStr);

        // assert
        resultTokenType.Should().Be(HackneyTokenType.MachineLegacy);
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_ReturnsUserType_ForUserToken()
    {
        // arrange
        var legacyUserToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
        var cognitoUserToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);

        // act
        var legacyInputTokenTypeResult = _sut.IdentifyHackneyToken(legacyUserToken.JwtString);
        var cognitoInputTokenTypeResult = _sut.IdentifyHackneyToken(cognitoUserToken.JwtString);

        // assert
        legacyInputTokenTypeResult.Should().Be(HackneyTokenType.User);
        cognitoInputTokenTypeResult.Should().Be(HackneyTokenType.User);
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_ReturnsUnknown_ForArbitraryPayload()
    {
        // arrange
        var unknownPayload = new { Title = "Sheep Detectives", Year = 2026 };
        var unknownJwtStr = TokenTestsHelper.GenerateBasicGeneralPayloadToken(unknownPayload, isSigned: true);

        // act
        var resultTokenType = _sut.IdentifyHackneyToken(unknownJwtStr);

        // assert
        resultTokenType.Should().Be(HackneyTokenType.Unknown);
        VerifyLog(_mockLogger, LogLevel.Warning, "Provided JWT did not match any known and expected token schema fields.", Times.Once());
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_HandlesBearerPrefix()
    {
        // arrange
        var irrelevantPayload = TokenTestsHelper.GenerateLegacyM2mTokenObj();
        var irrelevantValidJwt = TokenTestsHelper.GenerateLegacyM2mJwt(irrelevantPayload);
        var bearerJwt = "Bearer " + irrelevantValidJwt;

        // act
        var resultTokenType = _sut.IdentifyHackneyToken(bearerJwt);

        // assert
        resultTokenType.Should().NotBe(HackneyTokenType.Unknown);
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_ReturnsUnknownType_ForUnsignedLegacyM2mJwt()
    {
        // arrange
        var m2mPayload = TokenTestsHelper.GenerateLegacyM2mTokenObj();
        var m2mUnsignedJwt = TokenTestsHelper.GenerateBasicGeneralPayloadToken(m2mPayload, isSigned: false);

        // act
        var twoPartJwt = m2mUnsignedJwt.TrimEnd('.');
        var resultTwoPart = _sut.IdentifyHackneyToken(twoPartJwt);
        var resultWithTrailingDot = _sut.IdentifyHackneyToken(m2mUnsignedJwt); // 3rd part empty

        // assert
        resultTwoPart.Should().Be(HackneyTokenType.Unknown);
        resultWithTrailingDot.Should().Be(HackneyTokenType.Unknown);
        VerifyLog(_mockLogger, LogLevel.Warning, "JWT is not signed. Hackney tokens are always signed.", Times.Exactly(2));
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_ReturnsUnknownType_ForUnsignedUserJwt()
    {
        // arrange
        var userTokenObj = TokenTestsHelper.GenerateTestTokenObj(TokenSchema.Old);
        var unsignedJwt = TokenTestsHelper.GenerateBasicGeneralPayloadToken(userTokenObj, isSigned: false);

        // act
        var twoPartJwt = unsignedJwt.TrimEnd('.');
        var resultTwoPart = _sut.IdentifyHackneyToken(twoPartJwt);
        var resultWithTrailingDot = _sut.IdentifyHackneyToken(unsignedJwt); // 3rd part empty

        // assert
        resultTwoPart.Should().Be(HackneyTokenType.Unknown);
        resultWithTrailingDot.Should().Be(HackneyTokenType.Unknown);
        VerifyLog(_mockLogger, LogLevel.Warning, "JWT is not signed. Hackney tokens are always signed.", Times.Exactly(2));
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_ReturnsUnknown_ForTwoPartArbitraryPayload()
    {
        // arrange
        var unknownPayload = new { Title = "Sheep Detectives", Year = 2026 };
        var unsignedJwt = TokenTestsHelper.GenerateBasicGeneralPayloadToken(unknownPayload, isSigned: false);

        // act
        var twoPartJwt = unsignedJwt.TrimEnd('.');
        var resultTwoPart = _sut.IdentifyHackneyToken(twoPartJwt);
        var resultWithTrailingDot = _sut.IdentifyHackneyToken(unsignedJwt); // 3rd part empty

        // assert
        resultTwoPart.Should().Be(HackneyTokenType.Unknown);
        resultWithTrailingDot.Should().Be(HackneyTokenType.Unknown);
        VerifyLog(_mockLogger, LogLevel.Warning, "JWT is not signed. Hackney tokens are always signed.", Times.Exactly(2));
    }

    [Fact]
    public void TokenFactory_IdentifyHackneyToken_LogsWarning_OnExceptionDuringIdentification()
    {
        // arrange
        // craft a 3-part JWT where the payload part is invalid base64, causing an exception in Decode/Parse
        var badPayload = "!!!invalid-base64!!!";
        var badJwt = $"header.{badPayload}.signature";

        // act
        var result = _sut.IdentifyHackneyToken(badJwt);

        // assert
        result.Should().Be(HackneyTokenType.Unknown);
        VerifyLog(_mockLogger, LogLevel.Warning, "Token identification failed due to error or unknown token format.", Times.Once());
    }

    private static void VerifyLog(Mock<ILogger<TokenFactory>> mockLogger, LogLevel level, string expectedMessage, Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && (string.Empty + v.ToString()).Contains(expectedMessage)),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times);
    }
}
