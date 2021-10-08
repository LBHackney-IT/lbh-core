using FluentValidation;
using FluentValidation.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hackney.Core.Validation.AspNet
{
    public class UseErrorCodeInterceptor : IValidatorInterceptor
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public UseErrorCodeInterceptor()
        {
            _jsonOptions = CreateJsonOptions();
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

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

        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }

        public ValidationResult AfterAspNetValidation(
            ActionContext actionContext,
            IValidationContext validationContext,
            ValidationResult result)
        {
            if (result.Errors.Any())
            {
                var projection = result.Errors.Select(failure => ConstructFailure(failure));
                return new ValidationResult(projection);
            }

            return result;
        }
    }
}