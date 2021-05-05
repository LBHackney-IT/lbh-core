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
        private readonly string _userId = Guid.NewGuid().ToString();

        private readonly HttpContext _httpContext;
        private readonly Mock<ILogger<LoggingScopeMiddleware>> _mockLogger;

        public LoggingScopeMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers.Add(HeaderConstants.CorrelationId, new StringValues(_correlationId));
            _httpContext.Request.Headers.Add(HeaderConstants.UserId, new StringValues(_userId));

            _mockLogger = new Mock<ILogger<LoggingScopeMiddleware>>();
        }

        [Fact]
        public async Task InvokeAsyncTestBeginsLoggingScope()
        {
            var sut = new LoggingScopeMiddleware(null);
            await sut.InvokeAsync(_httpContext, _mockLogger.Object).ConfigureAwait(false);

            var expectedState = $"CorrelationId: {_correlationId}; UserId: {_userId}";
            _mockLogger.Verify(x => x.BeginScope(It.Is<object>(y => y.ToString() == expectedState)), Times.Once());
        }
    }
}
