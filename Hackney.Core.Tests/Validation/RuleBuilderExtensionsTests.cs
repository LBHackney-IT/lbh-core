using FluentAssertions;
using FluentValidation;
using Hackney.Core.Validation;
using System;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.Validation
{

    public class RuleBuilderExtensionsTests
    {
        private readonly InlineValidator<Dummy> _validator;

        public RuleBuilderExtensionsTests()
        {
            _validator = new TestValidator();
        }

        [Fact]
        public void NotXssStringTestNullRuleBuilderThrows()
        {
            IRuleBuilder<Dummy, string> ruleBuilder = null;
            Func<IRuleBuilderOptions<Dummy, string>> func = () => ruleBuilder.NotXssString();
            func.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NotXssStringTestCreatesXssValidator()
        {
            _validator.RuleFor(x => x.StringVal).NotXssString();

            var rule = (IValidationRule<Dummy>)_validator.First();
            Assert.IsType<XssValidator<Dummy, string>>(rule.Components.LastOrDefault()?.Validator);
            rule.Member.Name.Should().Be("StringVal");
        }
    }
}
