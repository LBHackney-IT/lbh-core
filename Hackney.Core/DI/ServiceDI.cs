using Hackney.Core.Http;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Hackney.Core.DI
{
    public static class ServiceDI
    {
        /// <summary>
        /// Helper method to ensure that the application's DI container is used to inject 
        /// the required components to use ISnsGateway.
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddSnsGateway(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.TryAddScoped<ISnsGateway, SnsGateway>();
            return serviceCollection;
        }

        /// <summary>
        /// Helper method to ensure that the application's DI container is used to inject 
        /// the required components to use IHttpContextWrapper.
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddHttpContextWrapper(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.TryAddScoped<IHttpContextWrapper, HttpContextWrapper>();
            return serviceCollection;
        }

        /// <summary>
        /// Helper method to ensure that the application's DI container is used to inject 
        /// the required components to use ITokenFactory.
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddTokenFactory(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.TryAddScoped<ITokenFactory, TokenFactory>();
            return serviceCollection;
        }
    }
}
