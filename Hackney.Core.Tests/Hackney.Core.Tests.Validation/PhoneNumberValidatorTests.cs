using FluentAssertions;
using FluentValidation;
using Hackney.Core.Validation;
using Xunit;

namespace Hackney.Core.Tests.Validation
{
    public class PhoneNumberValidatorTests
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

        [Theory]
        [InlineData(PhoneNumberType.UK)]
        [InlineData(PhoneNumberType.International)]
        public void PhoneNumberValidatorConstructorTest(PhoneNumberType type)
        {
            var sut = new PhoneNumberValidator<Dummy>(type);
            sut.Name.Should().Be("PhoneNumberValidator");
            sut.Type.Should().Be(type);
        }

        [Theory]
        [InlineData(PhoneNumberType.UK, null)]
        [InlineData(PhoneNumberType.International, null)]
        [InlineData(PhoneNumberType.UK, "")]
        [InlineData(PhoneNumberType.International, "")]
        public void PhoneNumberValidatorIsValidTestNoInputReturnsFalse(PhoneNumberType type, string input)
        {
            var ctx = CreateContext(input);
            var sut = new PhoneNumberValidator<Dummy>(type);
            sut.IsValid(ctx, input).Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("fgkjdhnfgkjndf")]
        [InlineData("(+447222)555555")]
        [InlineData("+44(7222)555555")]
        [InlineData("(0722) 5555555 #22")]
        public void PhoneNumberValidatorIsValidTestUKInvalidValueFails(string input)
        {
            var ctx = CreateContext(input);
            var sut = new PhoneNumberValidator<Dummy>(PhoneNumberType.UK);
            sut.IsValid(ctx, input).Should().BeFalse();
        }

        [Theory]
        [InlineData("+447222555555")]
        [InlineData("07222555555")]
        [InlineData("07222 555555")]
        [InlineData("+44 7222 555 555")]
        [InlineData("(0722) 5555555 #2222")]
        public void PhoneNumberValidatorIsValidTestUKInvalidValueSucceeds(string input)
        {
            var ctx = CreateContext(input);
            var sut = new PhoneNumberValidator<Dummy>(PhoneNumberType.UK);
            sut.IsValid(ctx, input).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("fgkjdhnfgkjndf")]
        [InlineData("(+447222)555555")]
        [InlineData("(0722) 5555555 #22")]
        public void PhoneNumberValidatorIsValidTestIntInvalidValueFails(string input)
        {
            var ctx = CreateContext(input);
            var sut = new PhoneNumberValidator<Dummy>(PhoneNumberType.International);
            sut.IsValid(ctx, input).Should().BeFalse();
        }

        [Theory]
        [InlineData("+32999999")]
        [InlineData("+1-99-999-99-99")]
        [InlineData("+33 99-999-99-99")]
        [InlineData("+7 999999999")]
        [InlineData("+44 999999999")]
        [InlineData("+353 99-9999999")]
        [InlineData("+1 99-999-9999")]
        [InlineData("0044999999999")]
        [InlineData("00353 99-999 9999")]
        public void PhoneNumberValidatorIsValidTestIntInvalidValueSucceeds(string input)
        {
            var ctx = CreateContext(input);
            var sut = new PhoneNumberValidator<Dummy>(PhoneNumberType.International);
            sut.IsValid(ctx, input).Should().BeTrue();
        }
    }
}
