using Hackney.Core.Http;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hackney.Core.DI
{
    public static class ServiceDI
    {
        public static IServiceCollection AddSnsGateway(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<ISnsGateway, SnsGateway>();

            return serviceCollection;
        }

        public static IServiceCollection AddHttpContextWrapper(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<IHttpContextWrapper, HttpContextWrapper>();

            return serviceCollection;
        }

        public static IServiceCollection AddTokenFactory(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<ITokenFactory, TokenFactory>();

            return serviceCollection;
        }
    }
}
