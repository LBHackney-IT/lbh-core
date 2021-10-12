using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Hackney.Core.Validation.AspNet
{
    public static class FluentValidationExtensions
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            return services.AddFluentValidation(Assembly.GetExecutingAssembly());
        }

        public static IServiceCollection AddFluentValidation(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (assemblies is null || !assemblies.Any()) throw new ArgumentNullException(nameof(assemblies));

            services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblies(assemblies));
            services.TryAddTransient<IValidatorInterceptor, UseErrorCodeInterceptor>();

            return services;
        }
    }
}