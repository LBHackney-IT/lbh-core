using System;
using System.Linq;
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hackney.Core.Authorization
{
    public class AuthorizeEndpointByGroups : TypeFilterAttribute
    {
        /// <summary>
        /// Authorise this endpoint using permitted groups
        /// </summary>
        /// <param name="permittedGroupsVariable">
        /// The name of the environment variable that stores the permitted groups for the endpoint
        /// </param>
        public AuthorizeEndpointByGroups(string permittedGroupsVariable) : base(typeof(TokenGroupsFilter))
        {
            Arguments = [permittedGroupsVariable];
        }
    }

    public class TokenGroupsFilter : IAuthorizationFilter
    {
        private readonly string[] _requiredGoogleGroups;
        private readonly ITokenFactory _tokenFactory;

        public TokenGroupsFilter(ITokenFactory tokenFactory, string permittedGroupsVariable)
        {
            _tokenFactory = tokenFactory;

            var requiredGooglepermittedGroupsVariable = Environment.GetEnvironmentVariable(permittedGroupsVariable);
            if (requiredGooglepermittedGroupsVariable is null) throw new EnvironmentVariableNullException(permittedGroupsVariable);

            _requiredGoogleGroups = requiredGooglepermittedGroupsVariable.Split(','); // Note: Env variable must not have spaces after commas
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tokenString = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Trim();

            if (tokenString is null || tokenString == string.Empty)
            {
                context.Result = new UnauthorizedObjectResult($"Missing or empty Authorization header value.");
                return;
            }

            var tokenType = _tokenFactory.IdentifyHackneyToken(tokenString);

            switch (tokenType)
            {
                case HackneyTokenType.User:
                    Token? userToken = _tokenFactory.DecodeStandardToken(tokenString);
                    if (userToken is null || !userToken.Groups.Any(g => _requiredGoogleGroups.Contains(g)))
                        context.Result = new UnauthorizedObjectResult($"User {userToken?.Name} is not authorized to access this endpoint.");
                    return;
                case HackneyTokenType.MachineLegacy:
                    // DO NOTHING. Legacy Machine tokens access is already authorized per-endpoint level at the lambda authorizer.
                    // As such, there's nothing additional left to check at the API level.
                    return;
                default:
                    context.Result = new UnauthorizedObjectResult($"Unknown token type has been encountered. Access denied!");
                    return;
            }
            // TODO: this would ideally have some sort of logging setup, however, adding logger as injectable dependency will break public contract
            // As such, this is left for the v2 of this package. For now, only the Machine Legacy token handling bug is fixed.
        }
    }
}
