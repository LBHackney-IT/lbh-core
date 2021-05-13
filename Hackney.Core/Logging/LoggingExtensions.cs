using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Hackney.Core.Logging
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configures the logging stack to use the AWS LambdaLogger.
        /// Console output wioll only be included if the ASPNETCORE_ENVIRONMENT environment variable
        /// value is not "Staging" or "Production".
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">A Configuration instance</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection ConfigureLambdaLogging(this IServiceCollection services, IConfiguration configuration)
        {
            // We rebuild the logging stack so as to ensure the console logger is not used in production.
            // See here: https://weblog.west-wind.com/posts/2018/Dec/31/Dont-let-ASPNET-Core-Default-Console-Logging-Slow-your-App-down
            services.AddLogging(config =>
            {
                // clear out default configuration
                config.ClearProviders();

                config.AddConfiguration(configuration.GetSection("Logging"));
                config.AddDebug();
                config.AddEventSourceLogger();

                // Create and populate LambdaLoggerOptions object
                var loggerOptions = new LambdaLoggerOptions
                {
                    IncludeCategory = false,
                    IncludeLogLevel = true,
                    IncludeNewline = true,
                    IncludeEventId = true,
                    IncludeException = true,
                    IncludeScopes = true
                };
                config.AddLambdaLogger(loggerOptions);

                var aspNetcoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if ((aspNetcoreEnvironment != EnvironmentName.Production)
                    && (aspNetcoreEnvironment != EnvironmentName.Staging))
                {
                    config.AddConsole();
                }
            });

            return services;
        }
    }
}
