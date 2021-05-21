using FluentAssertions;
using Hackney.Core.DynamoDb;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb
{
    public class PaginationDetailsTests
    {
        [Fact]
        public void DefaultConstructorTest()
        {
            var sut = new PaginationDetails();
            sut.NextToken.Should().BeNull();
            sut.HasNext.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("{}")]
        public void CustomConstructorTestEmptyToken(string token)
        {
            var sut = new PaginationDetails(token);
            sut.NextToken.Should().BeNull();
            sut.HasNext.Should().BeFalse();
        }

        [Theory]
        [InlineData("some value")]
        [InlineData("{ \"id\": \"123\", \"name\": \"some name\"  }")]
        public void CustomConstructorTestWithTokenValue(string token)
        {
            var sut = new PaginationDetails(token);
            sut.NextToken.Should().Be(Base64UrlEncoder.Encode(token));
            sut.HasNext.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("{}")]
        public void EncodeNextTokenTestEmptyToken(string token)
        {
            var sut = new PaginationDetails();
            sut.EncodeNextToken(token);
            sut.NextToken.Should().BeNull();
            sut.HasNext.Should().BeFalse();
        }

        [Theory]
        [InlineData("some value")]
        [InlineData("{ \"id\": \"123\", \"name\": \"some name\"  }")]
        public void EncodeNextTokenTestWithTokenValue(string token)
        {
            var sut = new PaginationDetails();
            sut.EncodeNextToken(token);
            sut.NextToken.Should().Be(Base64UrlEncoder.Encode(token));
            sut.HasNext.Should().BeTrue();
        }

        [Fact]
        public void DecodeTokensTestEmptyToken()
        {
            var sut = new PaginationDetails();
            sut.DecodeNextToken().Should().BeNull();
        }

        [Theory]
        [InlineData("some value")]
        [InlineData("{ \"id\": \"123\", \"name\": \"some name\"  }")]
        public void DecodeTokensTestWithTokenValue(string token)
        {
            var sut = new PaginationDetails(token);
            sut.DecodeNextToken().Should().Be(token);
        }
    }
}
