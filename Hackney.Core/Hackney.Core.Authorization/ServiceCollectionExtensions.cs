using Amazon.SimpleSystemsManagement;
using Hackney.Core.Authorization.Internal;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.Authorization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHackneyAuthorization(
            this IServiceCollection services,
            string ssmParameterName,
            IAmazonSimpleSystemsManagement? ssmClient = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ssmParameterName);

            // A second call would add another map and another loader, so SSM would be read twice.
            // Keep the first registration. Its parameter name is the one that is read.
            if (services.Any(descriptor => descriptor.ServiceType == typeof(PolicyGroupMap)))
                return services;

            services.AddTokenFactory();

            // Empty on purpose. The hosted service replaces inner dictionary before the server listens.
            services.AddSingleton(new PolicyGroupMap(
                new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)));

            // The guard above stops these from getting duplicated upon AddHackneyAuthorization being called twice
            services.AddHostedService(serviceProvider => new GroupDocumentLoader(
                serviceProvider.GetRequiredService<PolicyGroupMap>(),
                ssmParameterName,
                ssmClient));
            services.AddSingleton<IAuthorizationHandler, HybridAccessHandler>();

            // Fallback triggers when neither the endpoint method, nor its controller is decorated with auth attribute.
            // If you want an endpoint to be public for whatever reason, use [AllowAnonymous].
            // This package requires no other authorization middleware, so FallbackPolicy overwrites anything potentially set before.
            services.AddAuthorization(authOptions =>
            {
                authOptions.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => false)
                    .Build();

                authOptions.AddPolicy(AllowAccessToAttribute.PolicyName, policy =>
                    policy.Requirements.Add(new HybridAccessRequirement()));
            });

            // Denied request was authenticated by the Lambda authorizer before reaching API, therefore 403
            // is written in place of the built-in 401 challenge. Middleware resolves interface, not this internal
            // class, therefore Replace is needed to remove a handler the app may have registered before this call.
            services.Replace(ServiceDescriptor.Singleton<IAuthorizationMiddlewareResultHandler, ForbiddenAuthorizationResultHandler>());

            return services;
        }
    }
}
