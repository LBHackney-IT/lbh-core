using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Hackney.Core.Logging
{
    [ExcludeFromCodeCoverage]
    public static class LogCallAspectServices
    {
        public static IApplicationBuilder UseLogCall(this IApplicationBuilder builder)
        {
            ServiceProvider = builder.ApplicationServices;
            return builder;
        }

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
