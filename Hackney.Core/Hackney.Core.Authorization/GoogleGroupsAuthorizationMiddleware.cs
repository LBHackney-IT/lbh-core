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

        public async Task Invoke(HttpContext context, ITokenFactory tokenFactory)
        {
            var token = tokenFactory.Create(context.Request.Headers);
            if (token == null)
            {
                await HandleResponseAsync(context, HttpStatusCode.Unauthorized, "JWT token cannot be parsed!").ConfigureAwait(false);
                return;
            }

            if (token.Groups == null)
            {
                await HandleResponseAsync(context, HttpStatusCode.Forbidden, "JWT token should contain [groups] claim!").ConfigureAwait(false);
                return;
            }
            var requiredGoogleGroupsVariable = Environment.GetEnvironmentVariable("REQUIRED_GOOGL_GROUPS");
            if (requiredGoogleGroupsVariable == null)
            {
                await HandleResponseAsync(context, HttpStatusCode.InternalServerError, "Cannot resolve REQUIRED_GOOGL_GROUPS environment variable!").ConfigureAwait(false);
                return;
            }

            var requiredGoogleGroups = requiredGoogleGroupsVariable.Split(';');
            if (!token.Groups.Any(g => requiredGoogleGroups.Contains(g)))
            {
                if (token.Groups == null)
                {
                    await HandleResponseAsync(context, HttpStatusCode.Forbidden, "Forbidden").ConfigureAwait(false);
                }
            }

            await _next.Invoke(context).ConfigureAwait(false);
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

    public class BaseErrorResponse
    {
        public BaseErrorResponse() { }

        public BaseErrorResponse(int statusCode, string message, string details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details ?? string.Empty;
        }

        /// <summary>
        /// Status code
        /// </summary>
        /// <example>
        /// 400
        /// </example>
        public int StatusCode { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        /// <example>
        /// Model cannot be null
        /// </example>>
        public string Message { get; set; }

        /// <summary>
        /// Stack Trace of Exception
        /// </summary>
        public string Details { get; set; }
    }
}
