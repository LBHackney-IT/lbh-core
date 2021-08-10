using Amazon.DynamoDBv2.DocumentModel;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.DynamoDb.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Hackney.Core.DynamoDb.Tests.Converters
{
    public class DynamoDbObjectListConverterTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly DynamoDbObjectListConverter<SomeObject> _sut;

        private List<T> CreateObjectList<T>() where T : class
        {
            return new List<T>(new[]
            {
                _fixture.Create<T>(),
                _fixture.Create<T>(),
                _fixture.Create<T>()
            });
        }

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

        public DynamoDbObjectListConverterTests()
        {
            _sut = new DynamoDbObjectListConverter<SomeObject>();
        }

        [Fact]
        public void ToEntryTestNullValueReturnsNull()
        {
            _sut.ToEntry(null).Should().BeEquivalentTo(new DynamoDBNull());
        }

        [Fact]
        public void ToEntryTestEnumValueReturnsConvertedObjects()
        {
            var list = CreateObjectList<SomeObject>();
            _sut.ToEntry(list).Should().BeEquivalentTo(
                new DynamoDBList(list.Select(x => Document.FromJson(JsonSerializer.Serialize(x, CreateJsonOptions())))));
        }

        [Fact]
        public void ToEntryTestInvalidInputThrows()
        {
            List<SomeOtherObject> list = new List<SomeOtherObject>(new[] { _fixture.Create<SomeOtherObject>() });
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
        public void FromEntryTestObjectListReturnsConvertedList()
        {
            var list = CreateObjectList<SomeObject>();
            var dbEntry = new DynamoDBList(
                list.Select(x => Document.FromJson(JsonSerializer.Serialize(x, CreateJsonOptions()))));

            _sut.FromEntry(dbEntry).Should().BeEquivalentTo(list);
        }

        [Fact]
        public void FromEntryTestInputNotAListThrows()
        {
            DynamoDBEntry dbEntry = new Primitive { Value = "This is an error" };

            _sut.Invoking((c) => c.FromEntry(dbEntry))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromEntryTestWrongObjectsReturnsEmptyObjects()
        {
            var list = CreateObjectList<SomeOtherObject>();
            DynamoDBList dbEntry = new DynamoDBList(
                list.Select(x => Document.FromJson(JsonSerializer.Serialize(x, CreateJsonOptions()))));

            var expected = new List<SomeObject>(new[]
            {
                new SomeObject(),
                new SomeObject(),
                new SomeObject(),
            });
            _sut.FromEntry(dbEntry).Should().BeEquivalentTo(expected);
        }
    }
}
