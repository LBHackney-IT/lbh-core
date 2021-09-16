using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Hackney.Core.Middleware.Exception
{
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// Adds a custom exception handler middleware to the MVC request pipeline.
        /// This handler will log the exception and then return a standard exception response to the caller.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        [ExcludeFromCodeCoverage]
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            return app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    await HandleExceptions(context, logger).ConfigureAwait(false);
                });
            });
        }

        public static async Task HandleExceptions(HttpContext context, ILogger logger)
        {
            context.Response.ContentType = "application/json";
            string message = "Internal Server Error.";

            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null)
            {
                switch (contextFeature.Error)
                {
                    case System.Exception ex:
                        message = ex.Message;
                        break;
                    default:
                        break;
                }

                logger.LogError(contextFeature.Error, "Request failed.");
            }

            var correlationId = context.Request.Headers.GetHeaderValue(HeaderConstants.CorrelationId);
            var exceptionResult = new ExceptionResult(message, context.TraceIdentifier,
                correlationId, context.Response.StatusCode);
            await context.Response.WriteAsync(exceptionResult.ToString()).ConfigureAwait(false);
        }
    }
}
