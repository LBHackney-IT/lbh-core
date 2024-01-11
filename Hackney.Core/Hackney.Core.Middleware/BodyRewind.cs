using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace Hackney.Core.Middleware
{
    [ExcludeFromCodeCoverage]
    public sealed class BodyRewindMiddleware
    {
        private readonly RequestDelegate _next;

        public BodyRewindMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            await _next(context).ConfigureAwait(false);
        }
    }

    [ExcludeFromCodeCoverage]
    public static class BodyRewindExtensions
    {
        /// <summary>
        /// Enables the HttpRequset body to be access from within a controller method.
        /// Without this the request body is rendered unaccessible by other middleware steps.
        /// </summary>
        /// <param name="app">An App builder</param>
        /// <returns>An App builder</returns>
        public static IApplicationBuilder EnableRequestBodyRewind(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<BodyRewindMiddleware>();
        }

    }
}
