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

        private const string IntPhoneNumberRegEx = @"^[+]?([0-9]*[\.\s\-\(\)]|[0-9]+){6,24}$";

        private const string UkPhoneNumberRegEx
           = @"^(((\+44\s?\d{4}|\(?0\d{4}\)?)\s?\d{3}\s?\d{3})|((\+44\s?\d{3}|\(?0\d{3}\)?)\s?\d{3}\s?\d{4})|((\+44\s?\d{2}|\(?0\d{2}\)?)\s?\d{4}\s?\d{4}))(\s?\#(\d{4}|\d{3}))?$";

        private const string UkPostCode = 
            @"^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})$";

    }
}
