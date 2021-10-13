using Elasticsearch.Net;
using Hackney.Core.ElasticSearch.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using System;

namespace Hackney.Core.ElasticSearch
{
    /// <summary>
    /// Class implementing extension methods to assist with configuring ElasticSearch
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string DefaultESDomainUrl = "http://localhost:9200";

        /// <summary>
        /// Registers IElasticClient within the DI container using the domain url retrieved from configuration 
        /// using the supplied configuration key. The ElasticClient is configured with a SingleNodeConnectionPool.
        /// If no value is found in the configuration the default url of http://localhost:9200 is used.
        /// Also registers the IWildCardAppenderAndPrepender interface.
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The Configuration interface</param>
        /// <param name="configKey">The name of the key within the configuration containing the required elastic search domain url</param>
        /// <exception cref="System.ArgumentNullException">If any of the inputs are null or empty</exception>
        public static IServiceCollection ConfigureElasticSearch(this IServiceCollection services, IConfiguration configuration,
                                                  string configKey)
        {
            return ConfigureElasticSearch(services, configuration, configKey, new Uri(DefaultESDomainUrl));
        }

        /// <summary>
        /// Registers IElasticClient within the DI container using the domain url retrieved from configuration 
        /// using the supplied configuration key. The ElasticClient is configured with a SingleNodeConnectionPool.
        /// If no value is found in the configuration the supplied default uri is used.
        /// Also registers the IWildCardAppenderAndPrepender interface.
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The Configuration interface</param>
        /// <param name="configKey">The name of the key within the configuration containing the required elastic search domain url</param>
        /// <param name="esDomainUrl">The default domain url to use if no value is found in configuration</param>
        /// <exception cref="System.ArgumentNullException">If any of the inputs are null or empty</exception>
        public static IServiceCollection ConfigureElasticSearch(this IServiceCollection services, IConfiguration configuration,
                                                  string configKey, Uri esDomainUrl)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrEmpty(configKey)) throw new ArgumentNullException(nameof(configKey));
            if (esDomainUrl is null) throw new ArgumentNullException(nameof(esDomainUrl));

            var url = configuration.GetValue<string>(configKey);
            if (!string.IsNullOrEmpty(url))
                esDomainUrl = new Uri(url);

            var pool = new SingleNodeConnectionPool(esDomainUrl);
            var connectionSettings =
                new ConnectionSettings(pool)
                    .PrettyJson().ThrowExceptions().DisableDirectStreaming();
            var esClient = new ElasticClient(connectionSettings);
            services.TryAddSingleton<IElasticClient>(esClient);

            services.TryAddScoped<IWildCardAppenderAndPrepender, WildCardAppenderAndPrepender>();

            return services;
        }
    }
}
