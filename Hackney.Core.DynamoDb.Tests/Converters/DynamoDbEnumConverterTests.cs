using Amazon.DynamoDBv2.DocumentModel;
using FluentAssertions;
using Hackney.Core.DynamoDb.Converters;
using System;
using Xunit;

namespace Hackney.Core.DynamoDb.Tests.Converters
{
    public class DynamoDbEnumConverterTests
    {
        public enum Number { One, Two, Three, Four, Five }

        private readonly DynamoDbEnumConverter<Number> _sut;

        public DynamoDbEnumConverterTests()
        {
            _sut = new DynamoDbEnumConverter<Number>();
        }

        [Fact]
        public void ToEntryTestNullValueReturnsNull()
        {
            _sut.ToEntry(null).Should().BeEquivalentTo(new DynamoDBNull());
        }

        [Fact]
        public void ToEntryTestEnumValueReturnsConvertedValue()
        {
            var value = Number.Five;
            _sut.ToEntry(value).Should().BeEquivalentTo(new Primitive { Value = "Five" });
        }

        [Fact]
        public void ToEntryTestInvalidInputThrows()
        {
            _sut.Invoking((c) => c.ToEntry("This is an error"))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromEntryTestNullValueReturnsEnumDefault()
        {
            _sut.FromEntry(null).Should().BeEquivalentTo(default(Number));
        }

        [Fact]
        public void FromEntryTestDynamoDBNullReturnsEnumDefault()
        {
            _sut.FromEntry(new DynamoDBNull()).Should().BeEquivalentTo(default(Number));
        }

        [Fact]
        public void FromEntryTestEmptyStringValueReturnsEnumDefault()
        {
            DynamoDBEntry dbEntry = new Primitive { Value = string.Empty };
            _sut.FromEntry(dbEntry).Should().BeEquivalentTo(default(Number));
        }

        [Fact]
        public void FromEntryTestEnumValueReturnsConvertedValue()
        {
            var stringValue = "Three";
            DynamoDBEntry dbEntry = new Primitive { Value = stringValue };

            ((Number)_sut.FromEntry(dbEntry)).Should().Be(Number.Three);
        }

        [Fact]
        public void FromEntryTestInvalidInputThrows()
        {
            DynamoDBEntry dbEntry = new Primitive { Value = "This is an error" };

            _sut.Invoking((c) => c.FromEntry(dbEntry))
                .Should().Throw<ArgumentException>();
        }
    }
}
