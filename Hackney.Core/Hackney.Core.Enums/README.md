# Hackney.Core.Enums

This package provides enums for use by clients. 
These include:

* TargetType

This package also provides ready-made validators for these enums.
These include:
* [TargetTypeValidator](#TargetTypeValidator)
* [TargetTypeNameValidator](#TargetTypeNameValidator)



## TargetTypeValidator
The TargetTypeValidator uses [Fluent Validation](https://docs.fluentvalidation.net/en/latest/index.html#) `PropertyValidator` 
implementation that will check if a provided enum is a valid TargeType.


### Usage
Applications using this validator will need to have already configured their application to use Fluent Validation.
The validator is used through the `RuleBuilder` extension method `IsTargetType()` that can be used when defining a validation rule.

```csharp
using FluentValidation;
using Hackney.Core.Enums;

namespace SomeApi.Domain.Validation
{
    public class SomeRequestObjectValidator : AbstractValidator<SomeRequestObject>
    {
        public SomeRequestObjectValidator()
        {
            // TargetType is an enum
            RuleFor(x => x.TargetType).IsTargetType();
        }
    }
}
```

## TargetTypeNameValidator
The TargetTypeNameValidator uses [Fluent Validation](https://docs.fluentvalidation.net/en/latest/index.html#) `PropertyValidator` implementation that will check if a provided string is a valid TargeType.


### Usage
Applications using this validator will need to have already configured their application to use Fluent Validation.
The validator is used through the `RuleBuilder` extension method `IsTargetType()` that can be used when defining a validation rule.

```csharp
using FluentValidation;
using Hackney.Core.Enums;

namespace SomeApi.Domain.Validation
{
    public class SomeRequestObjectValidator : AbstractValidator<SomeRequestObject>
    {
        public SomeRequestObjectValidator()
        {
            // TargetType is a string
            RuleFor(x => x.TargetType).IsTargetType();
        }
    }
}
```