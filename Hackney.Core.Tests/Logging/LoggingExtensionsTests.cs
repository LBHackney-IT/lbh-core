using FluentAssertions;
using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Moq;
using System;
using Xunit;

namespace Hackney.Core.Tests.Logging
{
    public class LoggingExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("Development")]
        [InlineData("Staging")]
        [InlineData("Production")]
        [InlineData("SomeOtherName")]
        public void ConfigureLambdaLoggingTest(string envName)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", envName);
            bool shouldHaveConsoleLogger = (envName != EnvironmentName.Production) && (envName != EnvironmentName.Staging);

            var mockConfig = new Mock<IConfiguration>();
            var mockLoggingSection = new Mock<IConfigurationSection>();
            mockConfig.Setup(x => x.GetSection("Logging")).Returns(mockLoggingSection.Object);

            var services = new ServiceCollection();
            services.ConfigureLambdaLogging(mockConfig.Object);

            services.IsServiceRegistered<ILoggerProvider, DebugLoggerProvider>().Should().BeTrue();
            services.IsServiceRegistered<ILoggerProvider>("LambdaILoggerProvider").Should().BeTrue();
            services.IsServiceRegistered<ILoggerProvider>("EventSourceLoggerProvider").Should().BeTrue();
            services.IsServiceRegistered<ILoggerProvider, ConsoleLoggerProvider>().Should().Be(shouldHaveConsoleLogger);
        }
    }
}
