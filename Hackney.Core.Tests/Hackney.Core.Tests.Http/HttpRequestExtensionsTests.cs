using FluentAssertions;
using Hackney.Core.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Http
{
    public class HttpRequestExtensionsTests
    {
        private const string BodyData = "This is some test body data";

        [Fact]
        public async Task GetRawBodyStringAsyncTest()
        {
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.SetupGet(x => x.Body).Returns(new MemoryStream(Encoding.Default.GetBytes(BodyData)));
            var result = await mockRequest.Object.GetRawBodyStringAsync().ConfigureAwait(false);
            result.Should().Be(BodyData);
        }
    }
}