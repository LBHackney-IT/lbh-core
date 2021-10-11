using FluentAssertions;
using Hackney.Core.Validation;
using System;
using Xunit;

namespace Hackney.Core.Tests.Validation
{
    public class PhoneNumberRegExTests
    {
        [Fact]
        public void GetRegExTestUnknownThrows()
        {
            Action act = () => PhoneNumberRegEx.GetRegEx((PhoneNumberType)10);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetRegExTestUkReturnsRegEx()
        {
            var result = PhoneNumberRegEx.GetRegEx(PhoneNumberType.UK);
            result.Should().Be(PhoneNumberRegEx.UkPhoneNumberRegEx);
        }

        [Fact]
        public void GetRegExTestIntReturnsRegEx()
        {
            var result = PhoneNumberRegEx.GetRegEx(PhoneNumberType.International);
            result.Should().Be(PhoneNumberRegEx.IntPhoneNumberRegEx);
        }
    }
}
