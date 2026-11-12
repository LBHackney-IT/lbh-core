using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Hackney.Core.Authorization.Internal;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hackney.Core.Authorization
{
    public static class ServiceCollectionExtensions
    {
        public static async Task<IServiceCollection> AddHackneyAuthorizationAsync(
            this IServiceCollection services,
            string ssmParameterName,
            IAmazonSimpleSystemsManagement? ssmClient = null)
        {
            if (string.IsNullOrWhiteSpace(ssmParameterName))
                throw new ArgumentNullException(nameof(ssmParameterName));

            services.AddTokenFactory();

            // TODO: the SSM functionality will be needed in another nuget package, so it might make more sense
            // to have a dedicated ssm package with its own registration that we reference like we do with token factory.
            // This being an MVP, this will be left crude as is.
            using var client = ssmClient ?? new AmazonSimpleSystemsManagementClient();
            string jsonPayload;
            try
            {
                var request = new GetParameterRequest { Name = ssmParameterName, WithDecryption = true };
                var response = await client.GetParameterAsync(request);
                jsonPayload = response.Parameter.Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to fetch authorization groups from SSM parameter: '{ssmParameterName}'", ex);
            }

            // mapping the param store json response into the model the domain expects
            var rawDict = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonPayload);
            if (rawDict == null)
                throw new InvalidOperationException("SSM Parameter payload could not be deserialized.");

            var domainPolicies = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in rawDict)
            {
                domainPolicies[kvp.Key] = new HashSet<string>(kvp.Value, StringComparer.Ordinal);
            }

            // register user groups configuration object and the auth handler
            services.AddSingleton(new PolicyGroupMap(domainPolicies));
            services.AddSingleton<IAuthorizationHandler, HybridAccessHandler>();

            // Fallback triggers when neither the endpoint method, nor its controller is decorated with auth attribute.
            // If you want an endpoint to be public for whatever reason, use [AllowAnonymous].
            services.AddAuthorization(authOptions =>
            {
                authOptions.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => false)
                    .Build();

                authOptions.AddPolicy("HybridAccessPolicy", policy =>
                    policy.Requirements.Add(new HybridAccessRequirement()));
            });

            return services;
        }
    }
}
