using Hackney.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Hackney.Core.Testing
{
    /// <summary>
    /// This fixture & collection should be used for on all test classes that are testing classes that use
    /// the LogCall attribute.
    /// This is required because the AspectInjector framework operates at compile-time.
    /// This means that simply constructing an instance of a class that uses the attribute requires all of
    /// supporting objects used by the LogCallAspect to also be set up.
    /// </summary>

    public class LogCallAspectFixture
    {
        public Mock<ILogger<LogCallAspect>> MockLogger { get; private set; }

        public LogCallAspectFixture()
        {
            MockLogger = SetupLogCallAspect();
        }

        public static Mock<ILogger<LogCallAspect>> SetupLogCallAspect()
        {
            var mockLogger = new Mock<ILogger<LogCallAspect>>();
            var mockAspect = new Mock<LogCallAspect>(mockLogger.Object);
            var mockAppServices = new Mock<IServiceProvider>();
            var appBuilder = new Mock<IApplicationBuilder>();

            appBuilder.SetupGet(x => x.ApplicationServices).Returns(mockAppServices.Object);
            LogCallAspectServices.UseLogCall(appBuilder.Object);
            mockAppServices.Setup(x => x.GetService(typeof(LogCallAspect))).Returns(mockAspect.Object);
            return mockLogger;
        }
    }

    [CollectionDefinition("LogCall collection")]
    public class LogCallAspectFixtureCollection : ICollectionFixture<LogCallAspectFixture>
    { }

}

