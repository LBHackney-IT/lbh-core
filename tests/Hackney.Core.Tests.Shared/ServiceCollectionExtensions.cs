using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Hackney.Core.Tests.Shared
{
    public static class ServiceCollectionExtensions
    {
        private static bool IsServiceRegistered<TServiceType>(ServiceDescriptor sd, string implementationTypeName) where TServiceType : class
        {
            if (null != sd.ImplementationInstance)
            {
                return sd.ServiceType == typeof(TServiceType)
                    && sd.ImplementationInstance.GetType().Name == implementationTypeName;
            }

            return sd.ServiceType == typeof(TServiceType)
                && sd.ImplementationType?.Name == implementationTypeName;
        }

        public static bool IsServiceRegistered<TServiceType>(this ServiceCollection services, string implementationTypeName) where TServiceType : class
        {
            return services.Any(x => IsServiceRegistered<TServiceType>(x, implementationTypeName));
        }

        public static bool IsServiceRegistered<TServiceType, TImplementationType>(this ServiceCollection services) where TServiceType : class where TImplementationType : class
        {
            return services.Any(x => IsServiceRegistered<TServiceType>(x, typeof(TImplementationType).Name));
        }
    }
}
