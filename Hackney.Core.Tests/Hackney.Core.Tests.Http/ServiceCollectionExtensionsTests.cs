using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hackney.Core.Tests.Http
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHttpContextWrapperTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => Hackney.Core.Http.ServiceCollectionExtensions.AddHttpContextWrapper(services);
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
        public void AddApiGatewayTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => Hackney.Core.Http.ServiceCollectionExtensions.AddApiGateway(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddApiGatewayTestAddsRequiredTypes()
        {
            var services = new ServiceCollection();
            services.AddApiGateway();
            services.IsServiceRegistered<IApiGateway, ApiGateway>().Should().BeTrue();
        }
    }
}
