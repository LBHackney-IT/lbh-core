using Amazon.DynamoDBv2.DocumentModel;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.DynamoDb.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Hackney.Core.DynamoDb.Tests.Converters
{
    public class DynamoDbObjectConverterTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly DynamoDbObjectConverter<SomeObject> _sut;

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        public DynamoDbObjectConverterTests()
        {
            _sut = new DynamoDbObjectConverter<SomeObject>();
        }

        [Fact]
        public void ToEntryTestNullValueReturnsNull()
        {
            _sut.ToEntry(null).Should().BeEquivalentTo(new DynamoDBNull());
        }

        [Fact]
        public void ToEntryTestEnumValueReturnsConvertedValue()
        {
            var obj = _fixture.Create<SomeObject>();
            _sut.ToEntry(obj).Should().BeEquivalentTo(
                Document.FromJson(JsonSerializer.Serialize(obj, CreateJsonOptions())));
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
        public void FromEntryTestEnumValueReturnsConvertedValue()
        {
            var obj = _fixture.Create<SomeObject>();
            DynamoDBEntry dbEntry = Document.FromJson(
                JsonSerializer.Serialize(obj, CreateJsonOptions()));

            ((SomeObject)_sut.FromEntry(dbEntry)).Should().BeEquivalentTo(obj);
        }

        [Fact]
        public void FromEntryTestInvalidInputNotAnObjectTypeThrows()
        {
            DynamoDBEntry dbEntry = new Primitive { Value = "This is an error" };

            _sut.Invoking((c) => c.FromEntry(dbEntry))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromEntryTestInvalidInputWrongObjectReturnsEmptyObject()
        {
            var obj = _fixture.Create<SomeOtherObject>();
            DynamoDBEntry dbEntry = Document.FromJson(
                JsonSerializer.Serialize(obj, CreateJsonOptions()));

            _sut.FromEntry(dbEntry).Should().BeEquivalentTo(new SomeObject());
        }
    }

    public class SomeObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public bool Bool { get; set; }
    }

    public class SomeOtherObject
    {
        public Guid DocId { get; set; }
        public string Description { get; set; }
        public bool SomeBool { get; set; }
    }
}
