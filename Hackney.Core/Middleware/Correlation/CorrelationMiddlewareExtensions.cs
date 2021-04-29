using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace Hackney.Core.Middleware.Correlation
{
    [ExcludeFromCodeCoverage]
    public static class CorrelationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationMiddleware>();
        }
    }
}
