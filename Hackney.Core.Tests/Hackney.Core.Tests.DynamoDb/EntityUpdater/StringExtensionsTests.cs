using AwesomeAssertions;
using Hackney.Core.DynamoDb.EntityUpdater;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb.EntityUpdater;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToCamelCase_NullOrEmpty_ReturnsTheInput(string input)
    {
        // arrange
        var value = input;

        // act
        var result = value.ToCamelCase();

        // assert
        result.Should().Be(input);
    }

    [Fact]
    public void ToCamelCase_SingleCharacter_IsLowercased()
    {
        // arrange
        var value = "A";

        // act
        var result = value.ToCamelCase();

        // assert
        result.Should().Be("a");
    }

    [Fact]
    public void ToCamelCase_LowercasesOnly_TheFirstCharacter()
    {
        // arrange
        var value = "OrangeCat";

        // act
        var result = value.ToCamelCase();

        // assert
        result.Should().Be("orangeCat");
    }
}
