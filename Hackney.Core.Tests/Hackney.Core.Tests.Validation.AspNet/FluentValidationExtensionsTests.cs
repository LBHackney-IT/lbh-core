using FluentAssertions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Hackney.Core.Validation.AspNet;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hackney.Core.Tests.Validation.AspNet
{
    public class FluentValidationExtensionsTests
    {
        [Fact]
        public void AddFluentValidationTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => FluentValidationExtensions.AddFluentValidation(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddFluentValidationTestNullAssembliesThrows()
        {
            var services = new ServiceCollection();
            Action act = () => services.AddFluentValidation((Assembly[])null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddFluentValidationTestEmptyAssembliesThrows()
        {
            var services = new ServiceCollection();
            Action act = () => services.AddFluentValidation(Enumerable.Empty<Assembly>().ToArray());
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddFluentValidationTestAddsRequiredTypesFromExecutingAssembly()
        {
            var services = new ServiceCollection();
            services.AddFluentValidation();

            services.IsServiceRegistered<IValidatorInterceptor, UseErrorCodeInterceptor>().Should().BeTrue();
        }

        [Fact]
        public void AddFluentValidationTestAddsRequiredTypesFromSuppliedAssemblies()
        {
            var services = new ServiceCollection();
            services.AddFluentValidation(Assembly.GetAssembly(typeof(TestValidator)));

            services.IsServiceRegistered<IValidatorInterceptor, UseErrorCodeInterceptor>().Should().BeTrue();

            services.IsServiceImplementationRegistered<TestValidator>().Should().BeTrue();
        }
    }

    public class TestEntity { }
    public class TestValidator : AbstractValidator<TestEntity> { }

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

        private static bool IsServiceImplementationRegistered<TImplementationType>(ServiceDescriptor sd) where TImplementationType : class
        {
            var implementationTypeName = typeof(TImplementationType).Name;
            if (null != sd.ImplementationInstance)
            {
                return sd.ImplementationInstance.GetType().Name == implementationTypeName;
            }

            return sd.ImplementationType?.Name == implementationTypeName;
        }

        public static bool IsServiceRegistered<TServiceType>(this ServiceCollection services, string implementationTypeName) where TServiceType : class
        {
            return services.Any(x => IsServiceRegistered<TServiceType>(x, implementationTypeName));
        }

        public static bool IsServiceRegistered<TServiceType, TImplementationType>(this ServiceCollection services) where TServiceType : class where TImplementationType : class
        {
            return services.Any(x => IsServiceRegistered<TServiceType>(x, typeof(TImplementationType).Name));
        }

        public static bool IsServiceImplementationRegistered<TImplementationType>(this ServiceCollection services) where TImplementationType : class
        {
            return services.Any(x => IsServiceImplementationRegistered<TImplementationType>(x));
        }
    }
}