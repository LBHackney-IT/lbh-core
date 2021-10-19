using Amazon.SimpleNotificationService;
using FluentAssertions;
using Hackney.Core.Sns;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.Sns
{
    public class SnsInitilisationExtensionsTests
    {
        [Fact]
        public void ConfigureSnsTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => SnsInitilisationExtensions.ConfigureSns(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("false")]
        [InlineData("true")]
        public void ConfigureSnsTestNoLocalModeEnvVarUsesAWSService(string localModeEnvVar)
        {
            Environment.SetEnvironmentVariable("Sns_LocalMode", localModeEnvVar);

            ServiceCollection services = new ServiceCollection();
            services.ConfigureSns();

            services.Any(x => x.ServiceType == typeof(IAmazonSimpleNotificationService)).Should().BeTrue();
            var sd = services.First(x => x.ServiceType == typeof(IAmazonSimpleNotificationService));
            sd.Lifetime.Should().Be((localModeEnvVar == "true") ? ServiceLifetime.Singleton : ServiceLifetime.Scoped);
            (sd.ImplementationFactory is null).Should().Be((localModeEnvVar != "true"));

            Environment.SetEnvironmentVariable("Sns_LocalMode", null);
        }

        [Fact]
        public void ConfigureSnsTestRegistersServices()
        {
            string url = "http://localhost:8000";
            Environment.SetEnvironmentVariable("Localstack_SnsServiceUrl", url);
            Environment.SetEnvironmentVariable("Sns_LocalMode", "true");

            ServiceCollection services = new ServiceCollection();
            services.ConfigureSns();
            var serviceProvider = services.BuildServiceProvider();

            var amazonSns = serviceProvider.GetService<IAmazonSimpleNotificationService>();
            amazonSns.Should().NotBeNull();
            amazonSns.Config.ServiceURL.Should().Be(url);

            Environment.SetEnvironmentVariable("Localstack_SnsServiceUrl", null);
            Environment.SetEnvironmentVariable("Sns_LocalMode", null);
        }
    }
}
