using FluentAssertions;
using Hackney.Core.Http.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Hackney.Core.Tests.Http.Exceptions
{
    public class GetFromApiExceptionTests
    {
        [Fact]
        public void GetFromApiExceptionConstructorTest()
        {
            var type = "SomeEntityName";
            var id = Guid.NewGuid();
            var route = $"Some/route/{id}";
            var statusCode = HttpStatusCode.OK;
            var msg = "Some API error message";

            var ex = new GetFromApiException(type, route, id, statusCode, msg);
            ex.EntityType.Should().Be(type);
            ex.EntityId.Should().Be(id);
            ex.Route.Should().Be(route);
            ex.StatusCode.Should().Be(statusCode);
            ex.ResponseBody.Should().Be(msg);
            ex.Headers.Should().BeEmpty();
            ex.Message.Should().Be($"Failed to get {type} details for id {id}. Route: {route}; Status code: {statusCode}; Message: {msg}");
        }

        [Fact]
        public void GetFromApiExceptionConstructorWithHeadersTest()
        {
            var type = "SomeEntityName";
            var id = Guid.NewGuid();
            var route = $"Some/route/{id}";
            var statusCode = HttpStatusCode.OK;
            var msg = "Some API error message";
            var headers = new List<KeyValuePair<string, IEnumerable<string>>>
            {
                new KeyValuePair<string, IEnumerable<string>>("key", new string[] { "a-value" })
            };

            var ex = new GetFromApiException(type, route, headers, id, statusCode, msg);
            ex.EntityType.Should().Be(type);
            ex.EntityId.Should().Be(id);
            ex.Route.Should().Be(route);
            ex.StatusCode.Should().Be(statusCode);
            ex.ResponseBody.Should().Be(msg);
            ex.Headers.Should().BeEquivalentTo(headers);
            ex.Message.Should().Be($"Failed to get {type} details for id {id}. Route: {route}; Status code: {statusCode}; Message: {msg}");
        }
    }
}
