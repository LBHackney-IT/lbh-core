using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Hackney.Core.Testing.Shared
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

        /// <summary>
        /// Validates if the specified service is registered in the services collection
        /// </summary>
        /// <typeparam name="TServiceType">The service type to check</typeparam>
        /// <param name="services">The services collection</param>
        /// <param name="implementationTypeName">The expected type name of the implementation type</param>
        /// <returns>true if registered, false if not</returns>
        public static bool IsServiceRegistered<TServiceType>(this ServiceCollection services, string implementationTypeName) where TServiceType : class
        {
            return services.Any(x => IsServiceRegistered<TServiceType>(x, implementationTypeName));
        }

        /// <summary>
        /// Validates if the specified service is registered in the services collection
        /// </summary>
        /// <typeparam name="TServiceType">The service type to check</typeparam>
        /// <typeparam name="TImplementationType">The expected type of the implementation type</typeparam>
        /// <param name="services">The services collection</param>
        /// <returns>true if registered, false if not</returns>
        public static bool IsServiceRegistered<TServiceType, TImplementationType>(this ServiceCollection services) where TServiceType : class where TImplementationType : class
        {
            return services.Any(x => IsServiceRegistered<TServiceType>(x, typeof(TImplementationType).Name));
        }
    }
}
