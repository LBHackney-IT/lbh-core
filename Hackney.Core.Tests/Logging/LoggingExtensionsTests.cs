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
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.Logging
{
    public class LoggingExtensionsTests
    {
        private static bool IsServiceRegistered<T>(ServiceDescriptor sd, string typeName)
        {
            if (null != sd.ImplementationInstance)
            {
                return sd.ServiceType == typeof(T)
                    && sd.ImplementationInstance.GetType().Name == typeName;
            }

            return sd.ServiceType == typeof(T)
                && sd.ImplementationType?.Name == typeName;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConfigureLambdaLoggingTest(bool isDebug)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", isDebug ? Environments.Development : null);

            var mockConfig = new Mock<IConfiguration>();
            var mockLoggingSection = new Mock<IConfigurationSection>();
            mockConfig.Setup(x => x.GetSection("Logging")).Returns(mockLoggingSection.Object);

            var services = new ServiceCollection();
            services.ConfigureLambdaLogging(mockConfig.Object);

            services.Any(x => IsServiceRegistered<ILoggerProvider>(x, nameof(DebugLoggerProvider))).Should().BeTrue();
            services.Any(x => IsServiceRegistered<ILoggerProvider>(x, "LambdaILoggerProvider")).Should().BeTrue();
            services.Any(x => IsServiceRegistered<ILoggerProvider>(x, "EventSourceLoggerProvider")).Should().BeTrue();
            services.Any(x => IsServiceRegistered<ILoggerProvider>(x, nameof(ConsoleLoggerProvider))).Should().Be(isDebug);
        }
    }
}
