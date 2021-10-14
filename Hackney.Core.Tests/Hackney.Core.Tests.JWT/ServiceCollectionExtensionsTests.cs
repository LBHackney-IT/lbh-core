using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hackney.Core.Tests.JWT
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenFactoryTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => Hackney.Core.JWT.ServiceCollectionExtensions.AddTokenFactory(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddTokenFactoryTestAddsRequiredTypes()
        {
            var services = new ServiceCollection();
            services.AddTokenFactory();
            services.IsServiceRegistered<ITokenFactory, TokenFactory>().Should().BeTrue();
        }
    }
}
