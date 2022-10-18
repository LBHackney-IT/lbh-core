using FluentValidation;
using System;

namespace Hackney.Core.Enums
{
    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// Validation rule to verify that the specified TargetType property value contains a valid TargetType enum
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="ruleBuilder">The RuleBuilder</param>
        public static IRuleBuilderOptions<T, TargetType> IsTargetType<T>(this IRuleBuilder<T, TargetType> ruleBuilder)
        {
            if (ruleBuilder is null) throw new ArgumentNullException(nameof(ruleBuilder));
            return ruleBuilder.SetValidator(new TargetTypeValidator<T>());
        }

        /// <summary>
        /// Validation rule to verify that the specified string property value contains a valid TargetType enum
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="ruleBuilder">The RuleBuilder</param>
        public static IRuleBuilderOptions<T, string> IsTargetType<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            if (ruleBuilder is null) throw new ArgumentNullException(nameof(ruleBuilder));
            return ruleBuilder.SetValidator(new TargetTypeNameValidator<T>());
        }
    }
}