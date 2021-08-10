using System.Threading.Tasks;
using FluentAssertions;
using Hackney.Core.Middleware;
using Hackney.Core.Middleware.CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Xunit;

namespace Hackney.Core.Tests.Middleware.CorrelationId
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task DoesNotReplaceCorrelationIdIfOneExists()
        {
            // Arrange
            var sut = new CorrelationIdMiddleware(null);
            var httpContext = new DefaultHttpContext();
            var headerValue = "123";

            httpContext.Request.Headers.Add(HeaderConstants.CorrelationId, headerValue);

            // Act
            await sut.InvokeAsync(httpContext).ConfigureAwait(false);

            // Assert
            httpContext.Request.Headers[HeaderConstants.CorrelationId].Should().BeEquivalentTo(headerValue);
        }

        [Fact]
        public async Task AddsCorrelationIdIfOneDoesNotExist()
        {
            // Arrange
            var sut = new CorrelationIdMiddleware(null);
            var httpContext = new DefaultHttpContext();

            // Act
            await sut.InvokeAsync(httpContext).ConfigureAwait(false);

            // Assert
            httpContext.Request.Headers[HeaderConstants.CorrelationId].Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task RequestDelegateCalled()
        {
            var requestDelegateCalled = false;
            RequestDelegate next = (httpContext) =>
            {
                requestDelegateCalled = true;
                return Task.CompletedTask;
            };
            var sut = new CorrelationIdMiddleware(next);
            var httpContext = new DefaultHttpContext();

            // Act
            await sut.InvokeAsync(httpContext).ConfigureAwait(false);

            // Assert
            requestDelegateCalled.Should().BeTrue();
        }

        [Fact]
        public async Task AddsCorrelationIdToResponse()
        {
            // Arrange
            var feature = new DummyResponseFeature();
            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<IHttpResponseFeature>(feature);
            RequestDelegate next = async (ctx) =>
            {
                await feature.InvokeCallBack().ConfigureAwait(false);
            };
            var sut = new CorrelationIdMiddleware(next);

            // Act
            await sut.InvokeAsync(httpContext).ConfigureAwait(false);

            // Assert
            var response = httpContext.Response;
            response.Headers[HeaderConstants.CorrelationId].Should().HaveCountGreaterThan(0);
        }
    }
}
