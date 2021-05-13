using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Hackney.Core.Middleware.Logging
{
    public class LoggingScopeMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingScopeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<LoggingScopeMiddleware> logger)
        {
            var correlationId = context.Request.Headers.GetHeaderValue(HeaderConstants.CorrelationId);
            var userId = context.Request.Headers.GetHeaderValue(HeaderConstants.UserId);

            using (logger.BeginScope("CorrelationId: {CorrelationId}; UserId: {UserId}", correlationId, userId))
            {
                if (_next != null)
                    await _next(context).ConfigureAwait(false);
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public static class LoggingScopeMiddlewareExtensions
    {
        /// <summary>
        /// Adds the LoggingScope middleware to the MVC request pipeline
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseLoggingScope(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingScopeMiddleware>();
        }
    }
}
