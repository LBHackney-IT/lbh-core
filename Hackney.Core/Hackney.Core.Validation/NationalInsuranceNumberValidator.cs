using FluentValidation;
using FluentValidation.Validators;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Hackney.Core.Validation
{
    /// <summary>
    /// Fluent Validation validator to check if a property is a valid UK NI number
    /// </summary>
    public class NationalInsuranceNumberValidator<T> : RegularExpressionValidator<T>
    {
        public override string Name => "NationalInsuranceNumberValidator";
        private const string NiRegEx
            = @"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)(?:[A-CEGHJ-PR-TW-Z][A-CEGHJ-NPR-TW-Z])(?:\s*\d\s*){6}([A-D]|\s)$";

        public NationalInsuranceNumberValidator()
            : base(NiRegEx, RegexOptions.IgnoreCase)
        { }

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return base.IsValid(context, value);
        }

        [ExcludeFromCodeCoverage]
        protected override string GetDefaultMessageTemplate(string errorCode)
          => "{PropertyName} does not contain a valid NI number";
    }
}
