using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hackney.Core.Authorization
{
    public class GoogleGroupsAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        public GoogleGroupsAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, ITokenFactory tokenFactory)
        {
            var urlsEnvironmentVariable = Environment.GetEnvironmentVariable("URLS_TO_SKIP_AUTH");
            if (urlsEnvironmentVariable == null)
            {
                await HandleResponseAsync(httpContext, HttpStatusCode.InternalServerError, "URLS_TO_SKIP_AUTH environment variable is null. Please, set up URLS_TO_SKIP_AUTH variable!").ConfigureAwait(false);
                return;
            }
            var urlsToSkipAuth = urlsEnvironmentVariable.Split(";").ToList();

            if (!httpContext.Request.Path.HasValue)
            {
                await HandleResponseAsync(httpContext, HttpStatusCode.InternalServerError, "Cannot get Path value from request!").ConfigureAwait(false);
                return;
            }

            var requestUrl = httpContext.Request.Path.Value;
            var needToSkipAuth = urlsToSkipAuth.Any(url => url.Equals(requestUrl));
            if (needToSkipAuth == true)
            {
                await _next.Invoke(httpContext).ConfigureAwait(false);
                return;
            }

            var token = tokenFactory.Create(httpContext.Request.Headers);
            if (token == null)
            {
                await HandleResponseAsync(httpContext, HttpStatusCode.Unauthorized, "JWT token cannot be parsed!").ConfigureAwait(false);
                return;
            }

            if (token.Groups == null)
            {
                await HandleResponseAsync(httpContext, HttpStatusCode.Forbidden, "JWT token should contain [groups] claim!").ConfigureAwait(false);
                return;
            }
            var requiredGoogleGroupsVariable = Environment.GetEnvironmentVariable("REQUIRED_GOOGLE_GROUPS");
            if (requiredGoogleGroupsVariable == null)
            {
                await HandleResponseAsync(httpContext, HttpStatusCode.InternalServerError, "Cannot resolve REQUIRED_GOOGLE_GROUPS environment variable!").ConfigureAwait(false);
                return;
            }

            var requiredGoogleGroups = requiredGoogleGroupsVariable.Split(';');
            if (!token.Groups.Any(g => requiredGoogleGroups.Contains(g)))
            {
                await HandleResponseAsync(httpContext, HttpStatusCode.Forbidden, "JWT token should contain allowed group!").ConfigureAwait(false);
                return;
            }

            await _next.Invoke(httpContext).ConfigureAwait(false);
        }

        public async Task HandleResponseAsync(HttpContext context, HttpStatusCode code, string message)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)code;

            await response.WriteAsync(JsonConvert.SerializeObject(new BaseErrorResponse((int)code, message)))
                   .ConfigureAwait(false);
        }
    }
}
