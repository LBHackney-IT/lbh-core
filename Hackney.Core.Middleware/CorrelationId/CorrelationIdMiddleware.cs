using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace Hackney.Core.Middleware.CorrelationId
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationIdProvided =
                context.Request.Headers.TryGetValue(HeaderConstants.CorrelationId, out var correlationId);
            if (!correlationIdProvided)
            {
                correlationId = new StringValues(Guid.NewGuid().ToString());
                context.Request.Headers.Add(HeaderConstants.CorrelationId, correlationId);
            }

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(HeaderConstants.CorrelationId))
                {
                    context.Response.Headers.Add(HeaderConstants.CorrelationId, correlationId);
                }

                return Task.CompletedTask;
            });

            if (_next != null)
                await _next(context).ConfigureAwait(false);
        }
    }
}
