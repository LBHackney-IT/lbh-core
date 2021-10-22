using FluentAssertions;
using Hackney.Core.Validation.AspNet;
using System.Collections.Generic;
using Xunit;

namespace Hackney.Core.Tests.Validation.AspNet
{
    public class ValidationExceptionResponseTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Some error")]
        public void ValidationExceptionResponseConstructorTest(string message)
        {
            var dic = new Dictionary<string, List<string>>();
            var sut = new ValidationFailedResponse(dic, message);
            sut.Errors.Should().BeEquivalentTo(dic);
            sut.Message.Should().Be(message);
            sut.Status.Should().Be(400);
        }
    }
}
