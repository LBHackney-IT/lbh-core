using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Hackney.Core.Testing.Sns
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method to add the <see cref="ISnsFixture"/> and <see cref="ISnsFixture"/> to the DI container 
        /// so that it can be used in tests.
        /// </summary>
        /// <param name="services">The ServiceCollection</param>
        /// <returns>The ServiceCollection</returns>
        public static IServiceCollection ConfigureSnsFixture(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ISnsEventVerifier, SnsEventVerifier>();
            services.TryAddSingleton<ISnsFixture, SnsFixture>();
            return services;
        }
    }
}
