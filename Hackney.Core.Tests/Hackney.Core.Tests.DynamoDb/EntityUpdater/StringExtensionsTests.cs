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
        input.ToCamelCase().Should().Be(input);
    }

    [Fact]
    public void ToCamelCaseSingleCharacterIsLowercased()
    {
        "A".ToCamelCase().Should().Be("a");
    }

    [Fact]
    public void ToCamelCaseLowercasesOnlyTheFirstCharacter()
    {
        "OrangeCat".ToCamelCase().Should().Be("orangeCat");
    }
}
