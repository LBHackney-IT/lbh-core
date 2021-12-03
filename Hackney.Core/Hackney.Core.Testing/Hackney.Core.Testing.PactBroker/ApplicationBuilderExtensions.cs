using Microsoft.AspNetCore.Builder;

namespace Hackney.Core.Testing.PactBroker
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Extension method to add the required custom middleware used by pact broker verification
        /// </summary>
        /// <param name="app"></param>
        /// <param name="audience">The Audience value that will be set in the token added by the <see cref="AuthorizationTokenReplacementMiddleware"/> 
        /// to all broker calls made to the API endpoints.</param>
        /// <returns></returns>
        public static IApplicationBuilder UsePactBroker(this IApplicationBuilder app, string audience)
        {
            return app.UseMiddleware<ProviderStateMiddleware>(app.ApplicationServices)
                      .UseMiddleware<AuthorizationTokenReplacementMiddleware>(audience);
        }
    }
}
