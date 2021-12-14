using FluentAssertions;
using Hackney.Core.Authorization;
using System.Net;
using Xunit;

namespace Hackney.Core.Tests.Authorization
{
    public class BaseErrorResponseTests
    {
        [Fact]
        public void BaseErrorResponseConstructorNullDetailsIsEmpty()
        {
            var expectedMessage = "Message";
            var expectedStatusCode = (int) HttpStatusCode.Forbidden;
            string expectedDetails = null;

            var sut = new BaseErrorResponse(expectedStatusCode, expectedMessage, expectedDetails);

            sut.Details.Should().Be(string.Empty);
            sut.Message.Should().Be(expectedMessage);
            sut.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void BaseErrorResponseConstructorNonNullDetailsIsEmpty()
        {
            var expectedMessage = "Message";
            var expectedStatusCode = (int) HttpStatusCode.Forbidden;
            string expectedDetails = "Details";

            var sut = new BaseErrorResponse(expectedStatusCode, expectedMessage, expectedDetails);

            sut.Details.Should().Be(expectedDetails);
            sut.Message.Should().Be(expectedMessage);
            sut.StatusCode.Should().Be(expectedStatusCode);
        }
    }
}
