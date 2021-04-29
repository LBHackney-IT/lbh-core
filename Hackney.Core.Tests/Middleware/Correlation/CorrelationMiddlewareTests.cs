using FluentAssertions;
using Hackney.Core.Middleware.Correlation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Middleware.Correlation
{
    public class CorrelationMiddlewareTests
    {
        private CorrelationMiddleware _sut; 

        public CorrelationMiddlewareTests()
        {
            _sut = new CorrelationMiddleware(null);
        }

        [Fact]
        public async Task DoesNotReplaceCorrelationIdIfOneExists()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var headerValue = "123";

            httpContext.HttpContext.Request.Headers.Add(Constants.CorrelationId, headerValue);

            // Act
            await _sut.InvokeAsync(httpContext).ConfigureAwait(false);

            // Assert
            httpContext.HttpContext.Request.Headers[Constants.CorrelationId].Should().BeEquivalentTo(headerValue);
        }

        [Fact]
        public async Task AddsCorrelationIdIfOneDoesNotExist()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            // Act
            await _sut.InvokeAsync(httpContext).ConfigureAwait(false);

            // Assert
            httpContext.HttpContext.Request.Headers[Constants.CorrelationId].Should().HaveCountGreaterThan(0);
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
            _sut = new CorrelationMiddleware(next);
            var httpContext = new DefaultHttpContext();

            // Act
            await _sut.InvokeAsync(httpContext).ConfigureAwait(false);

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
            RequestDelegate next = async (ctx) => {
                await feature.InvokeCallBack().ConfigureAwait(false);
            };
            _sut = new CorrelationMiddleware(next);

            // Act
            await _sut.InvokeAsync(httpContext).ConfigureAwait(false);
            await httpContext.Response.StartAsync().ConfigureAwait(false);

            // Assert
            var response = httpContext.Response;
            response.Headers[Constants.CorrelationId].Should().HaveCountGreaterThan(0);
        }

        private class DummyResponseFeature : IHttpResponseFeature
        {
            public Stream Body { get; set; }
            public bool HasStarted { get { return hasStarted; } }
            public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
            public string ReasonPhrase { get; set; }
            public int StatusCode { get; set; }

            public void OnCompleted(Func<object, Task> callback, object state)
            {
                //...No-op
            }

            public void OnStarting(Func<object, Task> callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            bool hasStarted = false;
            Func<object, Task> callback;
            object state;

            public Task InvokeCallBack()
            {
                hasStarted = true;
                return callback(state);
            }
        }
    }
}
