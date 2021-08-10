using FluentAssertions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using Xunit;

namespace Hackney.Core.Tests.JWT
{
    public class TokenFactoryTests
    {
        private readonly Mock<IHeaderDictionary> _mockHeaders;
        private readonly string _tokenString = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";

        private readonly TokenFactory _sut;

        public TokenFactoryTests()
        {
            _mockHeaders = new Mock<IHeaderDictionary>();
            _mockHeaders.Setup(x => x["Authorization"]).Returns(_tokenString);

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
            var actualHeader = headerName ?? ITokenFactory.DefaultHeaderName;
            _mockHeaders.Setup(x => x[actualHeader]).Returns(_tokenString);

            var token = _sut.Create(_mockHeaders.Object);
            token.Email.Should().Be("e2e-testing@development.com");
            token.Exp.Should().Be(0);
            token.Groups.Should().BeEquivalentTo(new[] { "e2e-testing" });
            token.Iat.Should().Be(1623058232);
            token.Name.Should().Be("Tester");
            token.Nbf.Should().Be(0);
            token.Sub.Should().Be("115018116092098676113");
        }
    }
}
