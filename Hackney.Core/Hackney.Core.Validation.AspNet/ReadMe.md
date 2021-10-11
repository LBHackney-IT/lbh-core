# `Hackney.Core.Validation.AspNet` NuGet Package

The main use of this packages is to configure the use of FluentValidation within our API services.

Fluent Validation allows you to separate validation rules from your object model and helps you structure the rules so that they are nice and readable. The rules are also super easy to test.

#### UseErrorCodeInterceptor

The `UseErrorCodeInterceptor` class is used under the covers to make sure that any error code that is specified on a validation rule is actually included in the HTTP response. By default it just includes the error message and any code specified is left out. Clients should not need to invoke this class directly.

So when a validation rule does this:

```csharp

      RuleFor(x => x.ReasonForTermination).NotXssString()
                    .WithErrorCode(ErrorCodes.XssCheckFailure);

```
then the error code value for XssCheckFailure will actually be included in the error details in the HTTP response.

#### FluentValidationExtensions

The main purpose of this class is to have a default `IValidatorInterceptor`  to be used for all validators.

Usage

```csharp

  public void ConfigureServices(IServiceCollection services)
  {
    ...

    // This example will register any validators just in the application's assembly
    services.AddFluentValidation();

    // This example will register any validators in the assembly where the CreatePersonRequestObjectValidator is located
    // (but not the local assembly - if you need that then add it specifically.)
    services.AddFluentValidation(Assembly.GetAssembly(typeof(CreatePersonRequestObjectValidator)));

    ...
   }
  
```
The example above registers all the validators located in either the current assembly, or the specified assemblies
