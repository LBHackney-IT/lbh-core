using System;
using System.Linq;
using System.Threading.Tasks;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hackney.Core.Authorization.Internal
{
    internal class HybridAccessHandler : AuthorizationHandler<HybridAccessRequirement>
    {
        private readonly PolicyGroupMap _policyMap;
        private readonly ILogger<HybridAccessHandler> _logger;

        public HybridAccessHandler(PolicyGroupMap policyMap, ILogger<HybridAccessHandler> logger)
        {
            _policyMap = policyMap ?? throw new ArgumentNullException(nameof(policyMap));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HybridAccessRequirement requirement)
        {
            if (context.Resource is not HttpContext httpContext) return Task.CompletedTask;

            var accessRule = httpContext.GetEndpoint()?.Metadata.GetMetadata<AllowAccessToAttribute>();
            if (accessRule == null) return Task.CompletedTask;

            var authorizationHeader = httpContext.Request.Headers.Authorization.FirstOrDefault()?.Trim();
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                _logger.LogWarning("Access denied: missing or empty Authorization header.");
                return Task.CompletedTask;
            }

            // TODO: make the ITokenFactory not be scoped and instead match the lifecycle of authorization
            var tokenFactory = httpContext.RequestServices.GetService(typeof(ITokenFactory)) as ITokenFactory;
            if (tokenFactory == null)
            {
                _logger.LogError("Access denied: {Service} is not registered.", nameof(ITokenFactory));
                return Task.CompletedTask;
            }

            var isAllowed = tokenFactory.IdentifyHackneyToken(authorizationHeader) switch
            {
                HackneyTokenType.User => IsUserAllowed(tokenFactory, authorizationHeader, accessRule),
                HackneyTokenType.MachineLegacy => IsLegacyMachineAllowed(tokenFactory, authorizationHeader, accessRule),
                HackneyTokenType.CognitoM2M => IsCognitoM2MAllowed(tokenFactory, authorizationHeader, accessRule),
                _ => LogUnknownTokenDenied()
            };

            if (isAllowed) context.Succeed(requirement);

            return Task.CompletedTask;
        }

        private bool IsUserAllowed(ITokenFactory tokenFactory, string jwt, AllowAccessToAttribute accessRule)
        {
            if (string.IsNullOrEmpty(accessRule.User))
            {
                _logger.LogWarning("Access denied: user token used on an endpoint that does not allow user access.");
                return false;
            }

            var token = tokenFactory.DecodeStandardToken(jwt);
            if (token == null)
            {
                _logger.LogWarning("Access denied: user token could not be decoded.");
                return false;
            }

            if (!_policyMap.Policies.TryGetValue(accessRule.User, out var allowedGroups))
            {
                _logger.LogWarning("Access denied: no group policy is configured for key {PolicyKey}.", accessRule.User);
                return false;
            }

            if (token.Groups.Any(allowedGroups.Contains)) return true;

            _logger.LogWarning("Access denied: user token groups do not match policy {PolicyKey}.", accessRule.User);
            return false;
        }

        private bool IsLegacyMachineAllowed(ITokenFactory tokenFactory, string jwt, AllowAccessToAttribute accessRule)
        {
            // The lambda authorizer already checked which endpoint this token may call, so there's little left to check.
            // However, for added security and documentation, endpoints are required to have the 'LegacyMachine' flag
            // enabled so the legacy request token would be permitted access.
            if (!accessRule.LegacyMachine)
            {
                _logger.LogWarning("Access denied: legacy machine token used on an endpoint that does not allow legacy machine access.");
                return false;
            }

            var token = tokenFactory.Decode<LegacyMachineToken>(jwt);

            if (token == null)
            {
                // Probably won't happen, but adding an explicit guard anyway just in case.
                _logger.LogWarning("Access granted to a legacy machine token whose Id could not be determined.");
                return false;
            }

            _logger.LogInformation(
                "Access granted to legacy machine token {TokenId} (consumer {ConsumerName}).",
                token.Id, token.ConsumerName);

            return true;
        }

        private bool IsCognitoM2MAllowed(ITokenFactory tokenFactory, string jwt, AllowAccessToAttribute accessRule)
        {
            if (string.IsNullOrEmpty(accessRule.Machine))
            {
                _logger.LogWarning("Access denied: Cognito M2M token used on an endpoint that does not allow machine access.");
                return false;
            }

            var token = tokenFactory.DecodeCognitoM2MToken(jwt);
            if (token == null)
            {
                _logger.LogWarning("Access denied: Cognito M2M token could not be decoded.");
                return false;
            }

            // The Http verb and the target API have already been checked by the lambda authorizer, so the only
            // check left to do is the endpoint name being mentioned within any of the token scopes.
            if (token.Scopes.Any(scope =>
                    string.Equals(scope.EndpointName, accessRule.Machine, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            _logger.LogWarning(
                "Access denied: Cognito M2M client {ClientId} has no scope for endpoint {EndpointName}.",
                token.ClientId, accessRule.Machine);
            return false;
        }

        private bool LogUnknownTokenDenied()
        {
            _logger.LogWarning("Access denied: unknown or unsupported token type.");
            return false;
        }
    }
}
