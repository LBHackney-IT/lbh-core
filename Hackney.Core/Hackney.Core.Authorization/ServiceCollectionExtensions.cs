using Amazon.SimpleSystemsManagement;
using Hackney.Core.Authorization.Internal;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Hackney.Core.Authorization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHackneyAuthorization(
            this IServiceCollection services,
            string ssmParameterName,
            IAmazonSimpleSystemsManagement? ssmClient = null)
        {
            if (string.IsNullOrWhiteSpace(ssmParameterName))
                throw new ArgumentNullException(nameof(ssmParameterName));

            services.AddTokenFactory();

            // Empty on purpose. The hosted service replaces inner dictionary before the server listens.
            services.AddSingleton(new PolicyGroupMap(
                new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)));

            services.AddHostedService(serviceProvider => new GroupDocumentLoader(
                serviceProvider.GetRequiredService<PolicyGroupMap>(),
                ssmParameterName,
                ssmClient));
            services.AddSingleton<IAuthorizationHandler, HybridAccessHandler>();

            // Fallback triggers when neither the endpoint method, nor its controller is decorated with auth attribute.
            // If you want an endpoint to be public for whatever reason, use [AllowAnonymous].
            services.AddAuthorization(authOptions =>
            {
                authOptions.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => false)
                    .Build();

                authOptions.AddPolicy(AllowAccessToAttribute.PolicyName, policy =>
                    policy.Requirements.Add(new HybridAccessRequirement()));
            });

            // Ensure the denied requests return a 403 (forbidden) instead of 401 (unauthorized) to reflect that
            // the user request was authenticated as it was able to reach the API, but didn't have the permissions
            // to access the endpoint it was targetting.
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, ForbiddenAuthorizationResultHandler>();

            return services;
        }
    }
}
