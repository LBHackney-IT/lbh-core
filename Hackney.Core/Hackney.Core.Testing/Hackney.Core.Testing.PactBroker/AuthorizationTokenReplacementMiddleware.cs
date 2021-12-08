using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.PactBroker
{
    /// <summary>
    /// Middleware class used to add an Authorization token into calls made to the API endpoints by the pact broker.
    /// </summary>
    public class AuthorizationTokenReplacementMiddleware
    {
        private const string Authorization = "Authorization";

        private readonly RequestDelegate _next;
        private readonly string _audience;

        public AuthorizationTokenReplacementMiddleware(RequestDelegate next, string audience)
        {
            _next = next;
            _audience = audience;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey(Authorization))
            {
                // swap for a valid key
                string token = TokenGenerator.Generate(_audience);
                context.Request.Headers[Authorization] = $"Bearer {token}";
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}
