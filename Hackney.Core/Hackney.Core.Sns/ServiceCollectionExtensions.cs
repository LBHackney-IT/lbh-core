using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Hackney.Core.Sns;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SNS client and <see cref="ISnsGateway"/>.
    /// Local mode (<c>Sns_LocalMode</c> parses as <c>true</c>) registers a singleton client
    /// whose <c>ServiceURL</c> is <c>Localstack_SnsServiceUrl</c>. Otherwise the client is scoped
    /// and the SDK resolves the region. An existing client or gateway is left in place.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSnsGatewayAndClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = bool.TryParse(Environment.GetEnvironmentVariable("Sns_LocalMode"), out var localMode);

        if (localMode)
        {
            var snsUrl = Environment.GetEnvironmentVariable("Localstack_SnsServiceUrl");
            services.TryAddSingleton<IAmazonSimpleNotificationService>(_ =>
            {
                var clientConfig = new AmazonSimpleNotificationServiceConfig { ServiceURL = snsUrl };
                return new AmazonSimpleNotificationServiceClient(clientConfig);
            });
        }
        else
        {
            services.TryAddScoped<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
        }

        services.TryAddScoped<ISnsGateway, SnsGateway>();
        return services;
    }
}
