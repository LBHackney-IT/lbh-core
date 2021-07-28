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
        public static IServiceCollection AddSnsGateway(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.TryAddScoped<ISnsGateway, SnsGateway>();
            return serviceCollection;
        }

        public static IServiceCollection AddHttpContextWrapper(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.TryAddScoped<IHttpContextWrapper, HttpContextWrapper>();
            return serviceCollection;
        }

        public static IServiceCollection AddTokenFactory(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.TryAddScoped<ITokenFactory, TokenFactory>();
            return serviceCollection;
        }
    }
}
