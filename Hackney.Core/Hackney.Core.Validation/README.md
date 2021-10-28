# Hackney.Core.Validation

This package provides ready-made validators for use by clients. 
These include:

* [NationalInsuranceNumberValidator](#NationalInsuranceNumberValidator)
* [PhoneNumberValidator](#PhoneNumberValidator)
* [XssValidator](#XssValidator)


## NationalInsuranceNumberValidator
The NationalInsuranceNumberValidator class is [Fluent Validation](https://docs.fluentvalidation.net/en/latest/index.html#) `PropertyValidator` 
implementation that will check if a string property value is valid UK National Insurance number.
##### Note 
The validator _does_ tolerate spaces within the value, so these do not need to be removed before validating.

### Usage
Applications using this validator will need to have already configured their application to use Fluent Validation.
The validator is used through the `RuleBuilder` extension method `IsNationalInsuranceNumber()` that can be used when defining a validation rule.

```csharp
using FluentValidation;
using Hackney.Core.Validation;

namespace SomeApi.Domain.Validation
{
    public class SomeRequestObjectValidator : AbstractValidator<SomeRequestObject>
    {
        public SomeRequestObjectValidator()
        {
            RuleFor(x => x.NiNumber).IsNationalInsuranceNumber();

            // If the property is optional and could be null or empty.
            RuleFor(x => x.NiNumber).IsNationalInsuranceNumber();
                                    .When(x => !string.IsNullOrEmpty(x.NiNumber));
        }
    }
}
```


## PhoneNumberValidator
The PhoneNumberValidator class is [Fluent Validation](https://docs.fluentvalidation.net/en/latest/index.html#) `PropertyValidator` 
implementation that will check if a string property value is valid phone number.

### Usage
Applications using this validator will need to have already configured their application to use Fluent Validation.
The validator is used through the `RuleBuilder` extension method `IsPhoneNumber()` that can be used when defining a validation rule.

In the example, the `IsPhoneNumber` rule is applied to both the `Mobile` and `Work` properties. 
It is assumed that the `Mobile` property will not be empty byt the `Work` property could be.

```csharp
using FluentValidation;
using Hackney.Core.Validation;

namespace SomeApi.Domain.Validation
{
    public class SomeRequestObjectValidator : AbstractValidator<SomeRequestObject>
    {
        public SomeRequestObjectValidator()
        {
            RuleFor(x => x.Mobile).IsPhoneNumber(PhoneNumberType.UK);
            RuleFor(x => x.Work).IsPhoneNumber(PhoneNumberType.UK)
                                .When(x => !string.IsNullOrWhiteSpace(x.Work);
        }
    }
}
```


## XssValidator
The XssValidator class is [Fluent Validation](https://docs.fluentvalidation.net/en/latest/index.html#) `PropertyValidator` 
implementation that will check if a property has potentially dangerous content.

### Usage
Applications using this validator will need to have already configured their application to use Fluent Validation.
The validator is used through the `RuleBuilder` extension method `NotXssString()` that can be used when defining a validation rule.

In the example, the `NotXssString` rule is applied to all 3 string properties on the object.
```csharp
using FluentValidation;
using Hackney.Core.Validation;

namespace SomeApi.Domain.Validation
{
    public class SomeRequestObjectValidator : AbstractValidator<SomeRequestObject>
    {
        public SomeRequestObjectValidator()
        {
            RuleFor(x => x.Category).NotXssString();
            RuleFor(x => x.SubCategory).NotXssString();
            RuleFor(x => x.Description).NotXssString();
        }
    }
}
```
