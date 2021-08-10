using FluentAssertions;
using Hackney.Core.Middleware.Exception;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Xunit;

namespace Hackney.Core.Tests.Middleware.Exception
{
    public class ExceptionResultTests
    {
        private readonly string _errorMessage = "This is some error message";
        private readonly string _traceId = Guid.NewGuid().ToString();
        private readonly string _correlationId = Guid.NewGuid().ToString();

        [Fact]
        public void ExceptionResultTestConstructorWithNoStatusCode()
        {
            var sut = new ExceptionResult(_errorMessage, _traceId, _correlationId);
            sut.CorrelationId.Should().Be(_correlationId);
            sut.Message.Should().Be(_errorMessage);
            sut.StatusCode.Should().Be(ExceptionResult.DefaultStatusCode);
            sut.TraceId.Should().Be(_traceId);
        }

        [Theory]
        [InlineData(200)]
        [InlineData(404)]
        public void ExceptionResultTestConstructorWithStatusCode(int statusCode)
        {
            var sut = new ExceptionResult(_errorMessage, _traceId, _correlationId, statusCode);
            sut.CorrelationId.Should().Be(_correlationId);
            sut.Message.Should().Be(_errorMessage);
            sut.StatusCode.Should().Be(statusCode);
            sut.TraceId.Should().Be(_traceId);
        }

        [Fact]
        public void ExceptionResultToStringTest()
        {
            var sut = new ExceptionResult(_errorMessage, _traceId, _correlationId);
            var asJson = sut.ToString();

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            sut.Should().BeEquivalentTo(JsonConvert.DeserializeObject<ExceptionResult>(asJson, settings));
        }
    }
}
