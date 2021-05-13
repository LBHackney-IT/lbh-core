using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace Hackney.Core.Middleware.CorrelationId
{
    [ExcludeFromCodeCoverage]
    public static class CorrelationIdMiddlewareExtensions
    {
        /// <summary>
        /// Adds the CorrelationId middleware to the MVC request pipeline
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
