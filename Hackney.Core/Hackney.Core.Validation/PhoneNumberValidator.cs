using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Hackney.Core.Validation
{
    /// <summary>
    /// Fluent Validation validator to check if a property is a valid phone number
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    public class PhoneNumberValidator<T> : RegularExpressionValidator<T>
    {
        public override string Name => "PhoneNumberValidator";
        public PhoneNumberType Type { get; private set; }

        public PhoneNumberValidator(PhoneNumberType type)
            : base(PhoneNumberRegEx.GetRegEx(type))
        {
            Type = type;
        }

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return base.IsValid(context, value);
        }

        [ExcludeFromCodeCoverage]
        protected override string GetDefaultMessageTemplate(string errorCode)
          => "{PropertyName} does not contain a valid " 
            + Enum.GetName(typeof(PhoneNumberType), Type)
            + " phone number.";
    }
}
