using FluentAssertions;
using Hackney.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using Xunit;

namespace Hackney.Core.Middleware.Tests
{
    public class HttpHeadersExtensionsTests
    {
        private const string KEY = "someHeaderKey";
        private const string VALUE = "some value";

        private readonly Mock<IHeaderDictionary> _mockHeaders = new Mock<IHeaderDictionary>();

        [Fact]
        public void GetHeaderValueTestNullHeaders()
        {
            Func<string> func = () => HttpHeadersExtensions.GetHeaderValue(null, KEY);
            func.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetHeaderValueTestInvalidKey(string key)
        {
            Func<string> func = () => _mockHeaders.Object.GetHeaderValue(key);
            func.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetHeaderValueTestKeyNotFoundReturnsNull()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal)).Returns(false);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().BeNull();
        }

        [Fact]
        public void GetHeaderValueTestKeyFoundNullValue()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal)).Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().BeNull();
        }

        [Fact]
        public void GetHeaderValueTestKeyFoundEmptyString()
        {
            StringValues outVal = new StringValues("");
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal)).Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().Be(string.Empty);
        }

        delegate void SubmitCallback(string x, out StringValues y);

        [Fact]
        public void GetHeaderValueTestKeyFoundSingleValue()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal))
                .Callback(new SubmitCallback((string x, out StringValues y) => y = new StringValues(VALUE)))
                .Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().Be(VALUE);
        }

        [Fact]
        public void GetHeaderValueTestKeyFoundManyValueReturnsFirst()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal))
                .Callback(new SubmitCallback((string x, out StringValues y) => y = new StringValues(new[] { VALUE, "val 2", "val 3" })))
                .Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().Be(VALUE);
        }
    }
}
