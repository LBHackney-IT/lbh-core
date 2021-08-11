using Hackney.Core.Logging;
using Hackney.Core.Tests.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hackney.Core.Tests.Logging
{
    [Collection("LogCall collection")]
    public class LogCallAspectTests
    {
        private readonly Mock<ILogger<LogCallAspect>> _logger;
        private readonly LogCallAspect _sut;

        private readonly Type _type = typeof(DummyClass);
        private readonly string _methodName = "SomeMethodName";

        public LogCallAspectTests()
        {
            _logger = new Mock<ILogger<LogCallAspect>>();
            _sut = new LogCallAspect(_logger.Object);
        }

        private static Attribute[] BuildTriggers(LogLevel? level = null)
        {
            LogCallAttribute attribute = new LogCallAttribute();
            if (level.HasValue)
                attribute = new LogCallAttribute(level.Value);

            return (new List<Attribute> { attribute }).ToArray();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        public void LogCallAspectLogEnterTestLogsAsExpected(LogLevel? level)
        {
            var triggers = BuildTriggers(level);
            _sut.LogEnter(_type, _methodName, triggers);

            _logger.VerifyExact(level ?? LogLevel.Trace,
                $"STARTING {_type.Name}.{_methodName} method", Times.Once());
        }

        [Fact]
        public void LogCallAspectLogEnterTestNoAttributeLogsTrace()
        {
            _sut.LogEnter(_type, _methodName, new List<Attribute>().ToArray());

            _logger.VerifyExact(LogLevel.Trace,
                $"STARTING {_type.Name}.{_methodName} method", Times.Once());
        }

        [Theory]
        [InlineData(null)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        public void LogCallAspectLogExitTestLogsAsExpected(LogLevel? level)
        {
            var triggers = BuildTriggers(level);
            _sut.LogExit(_type, _methodName, triggers);

            _logger.VerifyExact(level ?? LogLevel.Trace,
                $"ENDING {_type.Name}.{_methodName} method", Times.Once());
        }

        [Fact]
        public void LogCallAspectLogExitTestNoAttributeLogsTrace()
        {
            _sut.LogExit(_type, _methodName, new List<Attribute>().ToArray());

            _logger.VerifyExact(LogLevel.Trace,
                $"ENDING {_type.Name}.{_methodName} method", Times.Once());
        }
    }

    public class DummyClass
    { }
}
