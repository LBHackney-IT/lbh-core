using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hackney.Core.DynamoDb.HealthCheck
{
    public static class DynamoDbHealthCheckExtensions
    {
        private const string Name = "DynamoDb";

        internal static IServiceCollection RegisterDynamoDbHealthCheck<T>(this IServiceCollection services) where T : class
        {
            return services.AddSingleton<IHealthCheck, DynamoDbHealthCheck<T>>();
        }

        internal static IHealthChecksBuilder AddDynamoDbHealthCheck<T>(this IHealthChecksBuilder builder) where T : class
        {
            return builder.AddCheck<DynamoDbHealthCheck<T>>(Name);
        }

        /// <summary>
        /// Adds a health check to verify connectivity to a DynamoDb table.
        /// </summary>
        /// <typeparam name="T">The database model class used to determine the DynamoDb table name.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
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
