using FluentAssertions;
using FluentValidation;
using Hackney.Core.Validation;
using Xunit;

namespace Hackney.Core.Tests.Validation
{
    public class NationalInsuranceNumberValidatorTests
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
        [InlineData(null)]
        [InlineData("")]
        [InlineData("fgkjdhnfgkjndf")]
        [InlineData("N 234234iojhilj")]
        [InlineData("AB1234567Z")]
        public void NationalInsuranceNumberValidatorIsValidTestFails(string input)
        {
            var ctx = CreateContext(input);
            var sut = new NationalInsuranceNumberValidator<Dummy>();
            sut.IsValid(ctx, input).Should().BeFalse();
        }

        [Theory]
        [InlineData("NZ335598D")]
        [InlineData("nz335598d")]
        [InlineData("NZ 33 55 98 D")]
        [InlineData("NZ 335598 D")]
        public void NationalInsuranceNumberValidatorIsValidTestValidValueSucceeds(string input)
        {
            var ctx = CreateContext(input);
            var sut = new NationalInsuranceNumberValidator<Dummy>();
            sut.IsValid(ctx, input).Should().BeTrue();
        }
    }
}
