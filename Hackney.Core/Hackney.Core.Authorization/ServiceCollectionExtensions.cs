using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.Authorization.Internal;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            var json = await FetchGroupDocument(ssmParameterName, ssmClient);
            services.AddSingleton(ReadGroupMap(json, ssmParameterName));
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

        private static PolicyGroupMap ReadGroupMap(string json, string ssmParameterName)
        {
            try
            {
                return PolicyGroupMapMapper.Map(json);
            }
            catch (InvalidGroupDocumentException ex)
            {
                throw new InvalidGroupDocumentException(
                    $"Failed to read authorization groups from SSM parameter: '{ssmParameterName}'. {ex.Message}", ex);
            }
        }

        // TODO: the SSM functionality will be needed in another nuget package, so it might make more sense
        // to have a dedicated ssm package with its own registration that we reference like we do with token factory.
        // This being an MVP, this will be left crude as is.
        private static async Task<string> FetchGroupDocument(
            string ssmParameterName,
            IAmazonSimpleSystemsManagement? ssmClient)
        {
            var ownsClient = ssmClient is null;
            var client = ssmClient ?? new AmazonSimpleSystemsManagementClient();
            try
            {
                var request = new GetParameterRequest { Name = ssmParameterName, WithDecryption = true };
                var response = await client.GetParameterAsync(request);
                return response.Parameter.Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to fetch authorization groups from SSM parameter: '{ssmParameterName}'", ex);
            }
            finally
            {
                if (ownsClient)
                    client.Dispose();
            }
        }
    }
}
