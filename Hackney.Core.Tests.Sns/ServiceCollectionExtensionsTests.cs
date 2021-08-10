using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Core.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hackney.Core.Tests.Sns
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddSnsGatewayTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => ServiceCollectionExtensions.AddSnsGateway(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddSnsGatewayTestAddsRequiredTypes()
        {
            var services = new ServiceCollection();
            services.AddSnsGateway();
            services.IsServiceRegistered<ISnsGateway, SnsGateway>().Should().BeTrue();
        }
    }
}
