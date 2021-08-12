using System;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hackney.Core.DynamoDb
{
    public static class SnsInitilisationExtensions
    {
        /// <summary>
        /// Configures an application to sns 
        /// </summary>
        /// <param name="services">A service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection ConfigureSns(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            bool localMode = false;
            _ = bool.TryParse(Environment.GetEnvironmentVariable("DynamoDb_LocalMode"), out localMode);

            if (localMode)
            {
                var snsUrl = Environment.GetEnvironmentVariable("Localstack_SnsServiceUrl");
                services.TryAddSingleton<IAmazonSimpleNotificationService>(sp =>
                {
                    var clientConfig = new AmazonSimpleNotificationServiceConfig { ServiceURL = snsUrl };
                    return new AmazonSimpleNotificationServiceClient(clientConfig);
                });
            }
            else
            {
                services.TryAddScoped<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            }

            return services;
        }
    }
}