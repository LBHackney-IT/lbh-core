using FluentAssertions;
using FluentValidation;
using Hackney.Core.Validation;
using Xunit;


namespace Hackney.Core.Validation.Tests
{
    public class XssValidatorTests
    {
        private static ValidationContext<Dummy> CreateContext(string propertyValue)
        {
            var dummy = new Dummy()
            {
                IntVal = 10,
                StringVal = propertyValue
            };
            return new ValidationContext<Dummy>(dummy);
        }

        [Fact]
        public void XssValidatorConstructorTest()
        {
            var sut = new XssValidator<Dummy, string>();
            sut.Name.Should().Be("XssValidator");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void XssValidatorIsValidTestNoInputReturnsTrue(string input)
        {
            var ctx = CreateContext(input);
            var sut = new XssValidator<Dummy, string>();
            sut.IsValid(ctx, input).Should().BeTrue();
        }

        [Fact]
        public void XssValidatorIsValidTestValidInputReturnsTrue()
        {
            var input = "A valid input string";
            var ctx = CreateContext(input);
            var sut = new XssValidator<Dummy, string>();
            sut.IsValid(ctx, input).Should().BeTrue();
        }

        [Fact]
        public void XssValidatorIsValidTestInvalidInputReturnsFalse()
        {
            var input = "An valid </br> input string";
            var ctx = CreateContext(input);
            var sut = new XssValidator<Dummy, string>();
            sut.IsValid(ctx, input).Should().BeFalse();
        }
    }
}
