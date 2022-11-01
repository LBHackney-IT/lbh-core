using FluentAssertions;
using FluentValidation;
using Hackney.Core.Enums;
using Xunit;
using System;

namespace Hackney.Core.Tests.Enums
{
    public class TargetTypeValidatorTests
    {
        private static ValidationContext<Dummy> CreateContext()
        {
            var dummy = new Dummy();
            return new ValidationContext<Dummy>(dummy);
        }

        [Fact]
        public void TargetTypeValidatorConstructorTest()
        {
            var sut = new TargetTypeValidator<Dummy>();
            sut.Name.Should().Be("TargetTypeValidator");
        }

        [Theory]
        [InlineData(TargetType.Person)]
        [InlineData(TargetType.Asset)]
        [InlineData(TargetType.Tenure)]
        [InlineData(TargetType.Repair)]
        [InlineData(TargetType.Process)]
        public void TargetTypeValidatorIsValidSucceeds(TargetType targetType)
        {
            var ctx = CreateContext();
            var sut = new TargetTypeValidator<Dummy>();
            sut.IsValid(ctx, targetType).Should().BeTrue();
        }

        [Fact]
        public void TargetTypeValidatorIsValidFails()
        {
            var ctx = CreateContext();
            var sut = new TargetTypeValidator<Dummy>();
            sut.IsValid(ctx, (TargetType)(-1)).Should().BeFalse();
        }
    }
}
