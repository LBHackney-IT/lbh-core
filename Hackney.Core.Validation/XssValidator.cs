using FluentValidation;
using FluentValidation.Validators;
using System.Diagnostics.CodeAnalysis;

namespace Hackney.Core.Validation
{
    /// <summary>
    /// Fluent Validation validator to check if a property has potentially harmful content
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    /// <typeparam name="TProperty">The property type</typeparam>
    public class XssValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        public override string Name => "XssValidator";

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            var text = value?.ToString();
            if (string.IsNullOrEmpty(text)) return true;

            if (CrossSiteScriptingValidation.IsDangerousString(text, out int pos))
            {
                context.MessageFormatter.AppendArgument("position", pos);
                return false;
            }
            return true;
        }

        [ExcludeFromCodeCoverage]
        protected override string GetDefaultMessageTemplate(string errorCode)
          => "{PropertyName} contains potentially dangerous characters at position {position}.";
    }
}
