using FluentAssertions;
using FluentValidation;
using Hackney.Core.Enums;
using Xunit;

namespace Hackney.Core.Tests.Enums
{
    public class TargetTypeNameValidatorTests
    {
        private static ValidationContext<Dummy> CreateContext()
        {
            var dummy = new Dummy();
            return new ValidationContext<Dummy>(dummy);
        }

        [Fact]
        public void TargetTypeNameValidatorConstructorTest()
        {
            var sut = new TargetTypeNameValidator<Dummy>();
            sut.Name.Should().Be("TargetTypeNameValidator");
        }

        [Theory]
        [InlineData("Person")]
        [InlineData("Asset")]
        [InlineData("Tenure")]
        [InlineData("Repair")]
        [InlineData("Process")]
        public void TargetTypeNameValidatorIsValidSucceeds(string targetTypeName)
        {
            var ctx = CreateContext();
            var sut = new TargetTypeNameValidator<Dummy>();
            sut.IsValid(ctx, targetTypeName).Should().BeTrue();
        }

        [Fact]
        public void TargetTypeNameValidatorIsValidFails()
        {
            var ctx = CreateContext();
            var sut = new TargetTypeNameValidator<Dummy>();
            sut.IsValid(ctx, "test").Should().BeFalse();
        }
    }
}
