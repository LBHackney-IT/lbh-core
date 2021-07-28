using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Core.Middleware;
using Hackney.Core.Middleware.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Middleware.Logging
{
    public class LoggingScopeMiddlewareTests
    {
        private readonly string _correlationId = Guid.NewGuid().ToString();
        private const string Email = "someone@test.com";
        private readonly Token _token;

        private readonly HttpContext _httpContext;
        private readonly Mock<ITokenFactory> _mockTokenFactory;
        private readonly Mock<ILogger<LoggingScopeMiddleware>> _mockLogger;

        public LoggingScopeMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers.Add(HeaderConstants.CorrelationId, new StringValues(_correlationId));

            _mockTokenFactory = new Mock<ITokenFactory>();
            _token = new Token() { Email = Email };
            _mockTokenFactory.Setup(x => x.Create(_httpContext.Request.Headers, ITokenFactory.DefaultHeaderName)).Returns(_token);

            _mockLogger = new Mock<ILogger<LoggingScopeMiddleware>>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(Email)]
        public async Task InvokeAsyncTestBeginsLoggingScope(string email)
        {
            _token.Email = email;
            var sut = new LoggingScopeMiddleware(null);
            await sut.InvokeAsync(_httpContext, _mockTokenFactory.Object, _mockLogger.Object).ConfigureAwait(false);

            var expectedState = $"CorrelationId: {_correlationId}; UserEmail: {email?? "(null)"}";
            _mockLogger.Verify(x => x.BeginScope(It.Is<object>(y => y.ToString() == expectedState)), Times.Once());
        }

        [Fact]
        public async Task InvokeAsyncTestBeginsLoggingScopeWithNoToken()
        {
            _mockTokenFactory.Setup(x => x.Create(_httpContext.Request.Headers, ITokenFactory.DefaultHeaderName)).Returns((Token)null);
            var sut = new LoggingScopeMiddleware(null);
            await sut.InvokeAsync(_httpContext, _mockTokenFactory.Object, _mockLogger.Object).ConfigureAwait(false);

            var expectedState = $"CorrelationId: {_correlationId}; UserEmail: (null)";
            _mockLogger.Verify(x => x.BeginScope(It.Is<object>(y => y.ToString() == expectedState)), Times.Once());
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
            var sut = new LoggingScopeMiddleware(next);
            var httpContext = new DefaultHttpContext();

            // Act
            await sut.InvokeAsync(httpContext, _mockTokenFactory.Object, _mockLogger.Object).ConfigureAwait(false);

            // Assert
            requestDelegateCalled.Should().BeTrue();
        }
    }
}
