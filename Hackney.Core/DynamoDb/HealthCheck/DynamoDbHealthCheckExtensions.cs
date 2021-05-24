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

        internal static IServiceCollection AddDynamoDbHealthCheck<T>(this IServiceCollection services) where T : class
        {
            services.RegisterDynamoDbHealthCheck<T>();
            services.AddHealthChecks()
                    .AddDynamoDbHealthCheck<T>();
            return services;
        }

        /// <summary>
        /// Adds a health check to verify connectivity to a DynamoDb table.
        /// </summary>
        /// <typeparam name="T">The database model class used to determine the DynamoDb table name.</typeparam>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddDynamoDbHealthCheck<T>(this IHealthChecksBuilder builder) where T : class
        {
            return builder.AddCheck<DynamoDbHealthCheck<T>>(Name);
        }
    }
}
