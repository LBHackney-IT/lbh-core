using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Core.Tests.Shared;
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
    }
}
