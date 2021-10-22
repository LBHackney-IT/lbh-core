using System.Collections.Generic;

namespace Hackney.Core.Validation.AspNet
{
    /// <summary>
    /// Response class used when validation done in code has failed and an error response
    /// has to be generated from a <see cref="FluentValidation.ValidationException"/> 
    /// or a <see cref="FluentValidation.Results.ValidationResult"/>.
    /// </summary>
    public class ValidationFailedResponse
    {
        /// <summary>
        /// Http status - 400 (i.e. BadRequest)
        /// </summary>
        public int Status => 400;

        /// <summary>
        /// The list of formatted errors from the ValidationResult
        /// </summary>
        public Dictionary<string, List<string>> Errors { get; private set; }

        /// <summary>
        /// A custom message. 
        /// If the instance was generated from a <see cref="FluentValidation.ValidationException"/> 
        /// then this will be the exception message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errors">The error information. A dictionary keyed on proeprty name</param>
        /// <param name="message">An optional message</param>
        public ValidationFailedResponse(Dictionary<string, List<string>> errors, string message = null)
        {
            Errors = errors;
            Message = message;
        }
    }
}
