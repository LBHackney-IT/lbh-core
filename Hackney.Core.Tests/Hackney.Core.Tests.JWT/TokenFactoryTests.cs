using FluentAssertions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.JWT
{
    public class TokenFactoryTests
    {
        private readonly Mock<IHeaderDictionary> _mockHeaders;
        private readonly TokenFactory _sut;

        public TokenFactoryTests()
        {
            var nonEmptyToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
            _mockHeaders = new Mock<IHeaderDictionary>();
            _mockHeaders.Setup(x => x["Authorization"]).Returns(nonEmptyToken.JwtString);

            _sut = new TokenFactory();
        }

        [Fact]
        public void TokenFactoryCreateTestNullHeadersThrows()
        {
            Action act = () => _sut.Create(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TokenFactoryCreateTestEmptyHeaderNameThrows()
        {
            Action act = () => _sut.Create(_mockHeaders.Object, "");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TokenFactoryCreateTestReturnsNullWhenNoAuthorizationHeader()
        {
            _mockHeaders.Reset();

            _sut.Create(_mockHeaders.Object).Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("some-header")]
        public void TokenFactoryCreateTestReturnsToken(string headerName)
        {
            // arrange
            var testToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Old);
            var actualHeader = headerName ?? ITokenFactory.DefaultHeaderName;

            _mockHeaders.Reset();
            _mockHeaders.Setup(x => x[actualHeader]).Returns(testToken.JwtString);

            // act 
            var decodedToken = _sut.Create(_mockHeaders.Object, actualHeader);

            // assert
            decodedToken.Email.Should().Be(testToken.TokenObj.Email);
            decodedToken.Exp.Should().Be(testToken.TokenObj.Exp);
            decodedToken.Groups.Should().BeEquivalentTo(testToken.TokenObj.Groups);
            decodedToken.Iat.Should().Be(testToken.TokenObj.Iat);
            decodedToken.Name.Should().Be(testToken.TokenObj.Name);
            decodedToken.Nbf.Should().Be(testToken.TokenObj.Nbf);
            decodedToken.Sub.Should().Be(testToken.TokenObj.Sub);
        }

        [Fact]
        public void TokenFactory_CreateMethod_MapsTheCognitoTokenCorrectly()
        {
            // arrange
            var headerName = "Authorization";
            var testToken = TokenTestsHelper.GenerateTestTokenPresentationJWT(TokenSchema.Cognito);
            var expectedGroupsArray = testToken.TokenObj.CustomGroups?.Split(TokenTestsHelper.CognitoTokenGoogleGroupsSeparator).ToArray();

            _mockHeaders.Reset();
            _mockHeaders.Setup(x => x[headerName]).Returns(testToken.JwtString);

            // act 
            var decodedToken = _sut.Create(headerDictionary: _mockHeaders.Object, headerName);

            // assert
            decodedToken.Email.Should().Be(testToken.TokenObj.Email);
            decodedToken.Exp.Should().Be(testToken.TokenObj.Exp);
            decodedToken.Groups.Should().BeEquivalentTo(expectedGroupsArray);
            decodedToken.Iat.Should().Be(testToken.TokenObj.Iat);
            decodedToken.Name.Should().Be(testToken.TokenObj.Name);
            decodedToken.Nbf.Should().Be(testToken.TokenObj.Nbf);
            decodedToken.Sub.Should().Be(testToken.TokenObj.Sub);
        }

        [Fact]
        public void TokenFactory_CreateMethod_ReturnsEmptyGroupsArrayWhen()
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
    }
}
