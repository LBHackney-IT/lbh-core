using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Hackney.Core.JWT;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Helper method to ensure that the application's DI container is used to inject 
    /// the required components to use ITokenFactory.
    /// </summary>
    /// <param name="serviceCollection">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTokenFactory(this IServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        // Used to register ILogger<TokenFactory> if it hasn't been registered already
        // together with other core logging services by package the consuming API's startup.
        serviceCollection.AddLogging();

        serviceCollection.TryAddScoped<ITokenFactory, TokenFactory>();

        return serviceCollection;
    }
}
