# `Hackney.Core.Validation.AspNet` NuGet Package

The main use of this packages is to configure the use of FluentValidation within our API services.

Fluent Validation allows you to separate validation rules from your object model and helps you structure the rules so that they are nice and readable. The rules are also super easy to test.

#### UseErroCodeInterceptor

This class is used to configure error code and error messages that would be provided when a validation error occurs. This allows the client to understand the validation error that has occured and was of resolving it. 

Usage
```csharp

      private ValidationFailure ConstructFailure(ValidationFailure failure)
      {
         var errorDetail = new
         {
             ErrorCode = failure.ErrorCode,
             ErrorMessage = failure.ErrorMessage,
             CustomState = failure.CustomState
         };
         var errorDetailString = JsonSerializer.Serialize(errorDetail, _jsonOptions);
         failure.ErrorMessage = errorDetailString;
         return failure;
      }
  
      public ValidationResult AfterAspNetValidation(ActionContext actionContext,
      IValidationContext validationContext,ValidationResult result)
      {
         if (result.Errors.Any())
         {
             var projection = result.Errors.Select(failure => ConstructFailure(failure));
             return new ValidationResult(projection);
         }

         return result;
      }
```
The first method `ConstructFailure` constructs the error message that would be provided if there is a validation error. 
The second method `AfterAspNetValidation` displays the error message when there is a validation error.

#### FluentValidationExtensions

The main purpose of this class is to have a default `IValidatorInterceptor`  to be used for all validators.

Usage

```csharp

   public static IServiceCollection AddFluentValidation(this IServiceCollection services)
   {
      return services.AddFluentValidation(Assembly.GetExecutingAssembly());
   }

   services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblies(assemblies));
   services.TryAddTransient<IValidatorInterceptor, UseErrorCodeInterceptor>();
  
```
The example above registers all the validators located in either the current assembly, or the specified assemblies
