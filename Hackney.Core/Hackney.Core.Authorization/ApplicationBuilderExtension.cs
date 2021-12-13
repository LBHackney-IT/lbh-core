using Microsoft.AspNetCore.Builder;
using System;

namespace Hackney.Core.Authorization
{
    public static class ApplicationBuilderExtension
    {
        /// <summary>
        /// Adds middleware to check in JWT token contains needed Google group
        /// </summary>
        /// <returns>The service collection</returns>
        public static IApplicationBuilder UseGoogleGroupAuthorization(this IApplicationBuilder app)
        {
            if (app is null) throw new ArgumentNullException(nameof(IApplicationBuilder));

            app.UseMiddleware<GoogleGroupsAuthorizationMiddleware>();

            return app;
        }
    }
}
