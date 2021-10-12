using FluentValidation;
using System;

namespace Hackney.Core.Validation
{
    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// Validation rule to verify that the speicified string property value does not have potentially dangerous content
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <typeparam name="TElement">The property type</typeparam>
        /// <param name="ruleBuilder">The RuleBuilder</param>
        public static IRuleBuilderOptions<T, TElement> NotXssString<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
        {
            if (ruleBuilder is null) throw new ArgumentNullException(nameof(ruleBuilder));

            return ruleBuilder.SetValidator(new XssValidator<T, TElement>());
        }

        /// <summary>
        /// Validation rule to verify that the specified string property value contains a valid phone number of the specified type
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="ruleBuilder">The RuleBuilder</param>
        /// <param name="type">The PhoneNumberType</param>
        public static IRuleBuilderOptions<T, string> IsPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder, PhoneNumberType type)
        {
            if (ruleBuilder is null) throw new ArgumentNullException(nameof(ruleBuilder));

            return ruleBuilder.SetValidator(new PhoneNumberValidator<T>(type));
        }
    }
}
