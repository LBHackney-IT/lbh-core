using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Hackney.Core.Validation.AspNet
{
    public static class ValidationFailedExtensions
    {
        /// <summary>
        /// Constructs an error response object when validation that has been performed manually in code has failed
        /// and a 400 repose is required.
        /// This method will construct the appropriate response object with the same format as that produced by the 
        /// integrated validation check perfoemd by MVC.
        /// </summary>
        /// <param name="validationException">A validation exception object</param>
        /// <returns>The response object</returns>
        /// <exception cref="System.ArgumentNullException">If the ValidationException is null</exception>
        public static ValidationFailedResponse ConstructResponse(this ValidationException validationException)
        {
            if (validationException is null) throw new ArgumentNullException(nameof(validationException));

            return ConstructResponse(validationException.Errors, validationException.Message);
        }

        /// <summary>
        /// Constructs an error response object when validation that has been performed manually in code has failed
        /// and a 400 repose is required.
        /// This method will construct the appropriate response object with the same format as that produced by the 
        /// integrated validation check perfoemd by MVC.
        /// </summary>
        /// <param name="validationResult">A validation result object</param>
        /// <returns>The response object</returns>
        /// <exception cref="System.ArgumentNullException">If the ValidationResult is null</exception>
        public static ValidationFailedResponse ConstructResponse(this ValidationResult validationResult)
        {
            if (validationResult is null) throw new ArgumentNullException(nameof(validationResult));

            return ConstructResponse(validationResult.Errors);
        }

        private static ValidationFailedResponse ConstructResponse(IEnumerable<ValidationFailure> errors, string message = null)
        {
            var errorDictionary = new Dictionary<string, List<string>>();
            foreach (var error in errors)
            {
                if (!errorDictionary.ContainsKey(error.PropertyName)) errorDictionary.Add(error.PropertyName, new List<string>());

                var errorObject = new
                {
                    ErrorCode = error.ErrorCode,
                    ErrorMessage = error.ErrorMessage,
                    CustomState = error.CustomState
                };

                errorDictionary[error.PropertyName].Add(JsonSerializer.Serialize(errorObject));
            }

            return new ValidationFailedResponse(errorDictionary, message);
        }

    }
}
