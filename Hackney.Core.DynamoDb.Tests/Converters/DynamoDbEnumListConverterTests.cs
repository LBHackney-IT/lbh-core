using Amazon.DynamoDBv2.DocumentModel;
using FluentAssertions;
using Hackney.Core.DynamoDb.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Hackney.Core.DynamoDb.Tests.Converters
{
    public class DynamoDbEnumListConverterTest
    {
        public enum Number { One, Two, Three, Four, Five }

        private readonly DynamoDbEnumListConverter<Number> _sut;

        public DynamoDbEnumListConverterTest()
        {
            _sut = new DynamoDbEnumListConverter<Number>();
        }

        [Fact]
        public void ToEntryTestNullValueReturnsNull()
        {
            _sut.ToEntry(null).Should().BeEquivalentTo(new DynamoDBNull());
        }

        [Theory]
        [InlineData(Number.One)]
        [InlineData(Number.One, Number.Three)]
        [InlineData(Number.One, Number.Three, Number.Four)]
        public void ToEntryTestEnumValueReturnsConvertedValues(params Number[] args)
        {
            var list = args.ToList();
            _sut.ToEntry(list).Should().BeEquivalentTo(
                new DynamoDBList(list.Select(x => new Primitive(Enum.GetName(typeof(Number), x)))));
        }

        [Fact]
        public void ToEntryTestInvalidInputThrows()
        {
            List<string> list = new List<string>(new[] { "Some", "Thing" });
            _sut.Invoking((c) => c.ToEntry(list))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromEntryTestNullValueReturnsNull()
        {
            _sut.FromEntry(null).Should().BeNull();
        }

        [Fact]
        public void FromEntryTestDynamoDBNullReturnsNull()
        {
            _sut.FromEntry(new DynamoDBNull()).Should().BeNull();
        }

        [Fact]
        public void FromEntryTestEnumValuesReturnsConvertedValues()
        {
            List<string> list = new List<string>(new[] { "One", "Four", "Five" });
            DynamoDBList dbEntry = DynamoDBList.Create(list);

            List<Number> expected = new List<Number>(new[] { Number.One, Number.Four, Number.Five });
            ((List<Number>)_sut.FromEntry(dbEntry)).Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void FromEntryTestInputNotAListThrows()
        {
            DynamoDBEntry dbEntry = new Primitive { Value = "This is an error" };

            _sut.Invoking((c) => c.FromEntry(dbEntry))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromEntryTestInvalidEnumInputThrows()
        {
            List<string> list = new List<string>(new[] { "One", "Nine", "Five" });
            DynamoDBList dbEntry = DynamoDBList.Create(list);

            _sut.Invoking((c) => c.FromEntry(dbEntry))
                .Should().Throw<ArgumentException>();
        }
    }
}
