using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hackney.Core.ElasticSearch.HealthCheck
{
    public static class ElasticSearchHealthCheckExtensions
    {
        private const string Name = "Elastic search";

        public static IServiceCollection RegisterElasticSearchHealthCheck(this IServiceCollection services)
        {
            services.TryAddSingleton<IHealthCheck, ElasticSearchHealthCheck>();
            return services;
        }

        public static IHealthChecksBuilder AddElasticSearchHealthCheck(this IHealthChecksBuilder builder)
        {
            return builder.AddCheck<ElasticSearchHealthCheck>(Name);
        }

        /// <summary>
        /// Adds a health check to verify connectivity to the Elastic Search instance
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddElasticSearchHealthCheck(this IServiceCollection services)
        {
            services.RegisterElasticSearchHealthCheck();
            services.AddHealthChecks()
                    .AddElasticSearchHealthCheck();
            return services;
        }
    }
}
