using FluentAssertions;
using Hackney.Core.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Hackney.Core.Tests.Http
{
    public class JsonOptionsTests
    {
        [Fact]
        public void CreateJsonOptionsTest()
        {
            var options = JsonOptions.Create();

            options.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
            options.WriteIndented.Should().BeTrue();
            options.Converters.Should().ContainEquivalentOf(new JsonStringEnumConverter());
        }
    }
}
