using FluentAssertions;
using Hackney.Core.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Hackney.Core.Http.Tests
{
    public class HttpContextWrapperTests
    {
        private readonly HttpContextWrapper _sut;

        public HttpContextWrapperTests()
        {
            _sut = new HttpContextWrapper();
        }

        [Fact]
        public void GetContextRequestHeadersTestsNullContextReturnsNull()
        {
            _sut.GetContextRequestHeaders(null).Should().BeNull();
        }

        [Fact]
        public void GetContextRequestHeadersTestsNullContextRequestReturnsNull()
        {
            var mockContext = new Mock<HttpContext>();
            _sut.GetContextRequestHeaders(mockContext.Object).Should().BeNull();
        }

        [Fact]
        public void GetContextRequestHeadersTestsNullHeadersReturnsNull()
        {
            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            _sut.GetContextRequestHeaders(mockContext.Object).Should().BeNull();
        }

        [Fact]
        public void GetContextRequestHeadersTestsReturnsHeaders()
        {
            var context = new DefaultHttpContext();
            _sut.GetContextRequestHeaders(context).Should().BeEquivalentTo(context.Request.Headers);
        }
    }
}
