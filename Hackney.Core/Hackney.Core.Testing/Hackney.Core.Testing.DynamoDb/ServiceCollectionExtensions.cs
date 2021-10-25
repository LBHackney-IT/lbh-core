using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Hackney.Core.Testing.DynamoDb
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method to add the <see cref="IDynamoDbFixture"/> to the DI container 
        /// so that it can be used in tests.
        /// </summary>
        /// <param name="services">The ServiceCollection</param>
        /// <returns>The ServiceCollection</returns>
        public static IServiceCollection ConfigureDynamoDbFixture(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IDynamoDbFixture, DynamoDbFixture>();
            return services;
        }
    }
}
