# `Hackney.Core.Validation.AspNet` NuGet Package

The main use of this packages is to configure the use of FluentValidation within our API services.

Fluent Validation allows you to separate validation rules from your object model and helps you structure the rules so that they are nice and readable. The rules are also super easy to test.

#### UseErroCodeInterceptor

We have customised our FluentValidation through the use of an interceptor. The `UseErrorCodeInterceptor` implements the `IValidatorInterceptor`.

This interface has two methods – `BeforeAspNetValidation` and `AfterAspNetValidation`.

`BeforeMvcValidation` is invoked after the appropriate validator has been selected but before it is invoked. One of the arguments passed to this method is a `ValidationContext` that will eventually be passed to the validator. The context has several properties including a reference to the object being validated. If we want to change which rules are going to be invoked (for example, by using a custom `ValidatorSelector`) then we can create a new `ValidationContext`, set its Selector property, and return that from the `BeforeAspNetValidation` method.

Likewise, `AfterAspNetValidation` occurs after validation has occurs. This time, we also have a reference to the result of the validation. Here we can do some additional processing on the error messages before they’re added to Model.

#### FluentValidationExtensions

The main purpose of this class is to have a default `IValidatorInterceptor`  to be used for all validators.

For example

```csharp

   services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblies(assemblies));
   services.TryAddTransient<IValidatorInterceptor, UseErrorCodeInterceptor>();
  
```
The example above customises all validators to use the `AddFluentValidation()` as well as the `IValidatorInteceptor`