using FluentAssertions;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Hackney.Core.Tests.Logging
{
    public class LogCallAttributeTests
    {
        [Fact]
        public void DefaultConstructorTestSetsLogLevelTrace()
        {
            var sut = new LogCallAttribute();
            sut.Level.Should().Be(LogLevel.Trace);
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Error)]
        public void CustomConstructorTestSetsLogLevel(LogLevel level)
        {
            var sut = new LogCallAttribute(level);
            sut.Level.Should().Be(level);
        }
    }
}
