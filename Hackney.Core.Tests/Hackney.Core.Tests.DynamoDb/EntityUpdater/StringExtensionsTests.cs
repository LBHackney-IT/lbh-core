using FluentAssertions;
using Hackney.Core.DynamoDb.EntityUpdater;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb.EntityUpdater;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToCamelCaseNullOrEmptyReturnsTheInput(string input)
    {
        // arrange
        var value = input;

        // act
        var result = value.ToCamelCase();

        // assert
        result.Should().Be(input);
    }

    [Fact]
    public void ToCamelCaseSingleCharacterIsLowercased()
    {
        // arrange
        var value = "A";

        // act
        var result = value.ToCamelCase();

        // assert
        result.Should().Be("a");
    }

    [Fact]
    public void ToCamelCaseLowercasesOnlyTheFirstCharacter()
    {
        // arrange
        var value = "OrangeCat";

        // act
        var result = value.ToCamelCase();

        // assert
        result.Should().Be("orangeCat");
    }
}
