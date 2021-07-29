using FluentAssertions;
using Hackney.Core.DI;
using Hackney.Core.Http;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hackney.Core.Tests.DI
{
    public class ServiceDITests
    {
        [Fact]
        public void AddSnsGatewayTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => ServiceDI.AddSnsGateway(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddSnsGatewayTestAddsRequiredTypes()
        {
            var services = new ServiceCollection();
            services.AddSnsGateway();
            services.IsServiceRegistered<ISnsGateway, SnsGateway>().Should().BeTrue();
        }

        [Fact]
        public void AddHttpContextWrapperTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => ServiceDI.AddHttpContextWrapper(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddHttpContextWrapperTestAddsRequiredTypes()
        {
            var services = new ServiceCollection();
            services.AddHttpContextWrapper();
            services.IsServiceRegistered<IHttpContextWrapper, HttpContextWrapper>().Should().BeTrue();
        }

        [Fact]
        public void AddTokenFactoryTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => ServiceDI.AddTokenFactory(services);
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
