using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Hackney.Core.Logging
{
    [ExcludeFromCodeCoverage]
    public static class LogCallAspectServices
    {
        /// <summary>
        /// Helper method to ensure that the application's DI container is used to inject 
        /// a logger instance into the LogCallAspect.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseLogCall(this IApplicationBuilder builder)
        {
            ServiceProvider = builder.ApplicationServices;
            return builder;
        }

        /// <summary>
        /// Helper method to add the LogCallAspect to the DI container.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddLogCallAspect(this IServiceCollection services)
        {
            services.AddTransient<LogCallAspect>();
            return services;
        }

        public static IServiceProvider ServiceProvider { get; private set; }

        static LogCallAspectServices() { }

        public static object GetInstance(Type type) => ServiceProvider.GetService(type);
    }
}
