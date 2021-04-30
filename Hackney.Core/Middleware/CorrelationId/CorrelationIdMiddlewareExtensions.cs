using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace Hackney.Core.Middleware.CorrelationId
{
    [ExcludeFromCodeCoverage]
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
