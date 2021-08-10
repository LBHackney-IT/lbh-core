using Amazon.DynamoDBv2.DocumentModel;
using FluentAssertions;
using Hackney.Core.DynamoDb.Converters;
using System;
using Xunit;

namespace Hackney.Core.DynamoDb.Tests.Converters
{
    public class DynamoDbDateTimeConverterTests
    {
        private readonly DynamoDbDateTimeConverter _sut;

        public DynamoDbDateTimeConverterTests()
        {
            _sut = new DynamoDbDateTimeConverter();
        }

        [Fact]
        public void ToEntryTestNullValueReturnsNull()
        {
            _sut.ToEntry(null).Should().BeEquivalentTo(new DynamoDBNull());
        }

        [Fact]
        public void ToEntryTestDateTimeReturnsConvertedValue()
        {
            DateTime now = DateTime.UtcNow;
            _sut.ToEntry(now).Should().BeEquivalentTo(new Primitive { Value = now.ToString(DynamoDbDateTimeConverter.DATEFORMAT) });
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

        [Fact]
        public void FromEntryTestDateTimeReturnsConvertedValue()
        {
            DateTime now = DateTime.UtcNow;
            DynamoDBEntry dbEntry = new Primitive { Value = now.ToString(DynamoDbDateTimeConverter.DATEFORMAT) };

            ((DateTime)_sut.FromEntry(dbEntry)).Should().BeCloseTo(now);
        }

        [Fact]
        public void FromEntryTestDateOnlyReturnsConvertedValue()
        {
            DateTime now = DateTime.UtcNow;
            DynamoDBEntry dbEntry = new Primitive { Value = now.Date.ToString() };

            ((DateTime)_sut.FromEntry(dbEntry)).Should().BeCloseTo(now.Date);
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
