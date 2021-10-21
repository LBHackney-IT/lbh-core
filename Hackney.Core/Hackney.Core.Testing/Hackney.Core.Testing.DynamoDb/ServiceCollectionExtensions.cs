using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hackney.Core.Testing.DynamoDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDynamoDbFixture(this IServiceCollection services)
        {
            services.TryAddSingleton<IDynamoDbFixture, DynamoDbFixture>();
            return services;
        }
    }
}
