using FluentValidation.Validators;


namespace Hackney.Core.Enums
{
   /// <summary>
    /// Fluent Validation validator to check if a property is a valid TargetType
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    public class TargetTypeNameValidator<T> : StringEnumValidator<T>
    {
        public TargetTypeNameValidator()
            : base(typeof(TargetType), true)
        {
        }

        public override string Name => "TargetTypeNameValidator";      
    }    
}