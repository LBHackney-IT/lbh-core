using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Hackney.Core.Authorization.Internal
{
    /// <summary>
    /// Writes the HTTP response for an authorization decision.
    /// <see cref="AuthorizationMiddleware"/> calls this after a policy has run. The built-in handler
    /// treats a failed policy for an anonymous caller as a request to log in, which is a 401, and it
    /// needs a registered login handler to write that status. This package leaves the caller anonymous
    /// as the Lambda authorizer has already authenticated the request. A denial here means the caller
    /// is not allowed, so the response is 403, and no login handler is involved.
    /// </summary>
    internal class ForbiddenAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Succeeded)
            {
                await next(context);
                return;
            }

            // Since authentication is performed by lambda authorizer, the user within HttpContext
            // of this authorization package is not set. As such, on policy failure, this causes the
            // ASP.NET to return 401 instead of 403. This fixes that issue by making any policy failure
            // result as 403 considering the request wouldn't have reached the API had it wasn't authenticated.
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}
