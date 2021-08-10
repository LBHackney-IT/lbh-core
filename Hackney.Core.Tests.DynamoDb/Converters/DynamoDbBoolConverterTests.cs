using Amazon.DynamoDBv2.DocumentModel;
using FluentAssertions;
using Hackney.Core.DynamoDb.Converters;
using System;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb.Converters
{
    public class DynamoDbBoolConverterTests
    {
        private readonly DynamoDbBoolConverter _sut;

        public DynamoDbBoolConverterTests()
        {
            _sut = new DynamoDbBoolConverter();
        }

        [Fact]
        public void ToEntryTestNullValueReturnsNull()
        {
            _sut.ToEntry(null).Should().BeEquivalentTo(new DynamoDBNull());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToEntryTestBoolReturnsConvertedValue(bool input)
        {
            _sut.ToEntry(input).Should().BeEquivalentTo(new DynamoDBBool(input));
        }

        [Fact]
        public void ToEntryTestInvalidInputThrows()
        {
            _sut.Invoking((c) => c.ToEntry("This is an error"))
                .Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void FromEntryTestNullValueReturnsNull()
        {
            _sut.FromEntry(null).Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromEntryTestBoolReturnsConvertedValue(bool input)
        {
            DynamoDBEntry dbEntry = new DynamoDBBool(input);

            ((bool)_sut.FromEntry(dbEntry)).Should().Be(input);
        }

        [Fact]
        public void FromEntryTestInvalidInputThrows()
        {
            DynamoDBEntry dbEntry = new Primitive { Value = "This is an error" };

            _sut.Invoking((c) => c.FromEntry(dbEntry))
                .Should().Throw<FormatException>();
        }
    }
}
