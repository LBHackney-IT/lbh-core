using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hackney.Core.DynamoDb.HealthCheck
{
    public static class DynamoDbHealthCheckExtensions
    {
        private const string Name = "DynamoDb";

        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection RegisterDynamoDbHealthCheck<T>(this IServiceCollection services) where T : class
        {
            return services.AddSingleton<IHealthCheck, DynamoDbHealthCheck<T>>();
        }

        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddDynamoDbHealthCheck<T>(this IHealthChecksBuilder builder) where T : class
        {
            return builder.AddCheck<DynamoDbHealthCheck<T>>(Name);
        }

        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDynamoDbHealthCheck<T>(this IServiceCollection services) where T : class
        {
            services.RegisterDynamoDbHealthCheck<T>();
            services.AddHealthChecks()
                    .AddDynamoDbHealthCheck<T>();
            return services;
        }
    }
}
