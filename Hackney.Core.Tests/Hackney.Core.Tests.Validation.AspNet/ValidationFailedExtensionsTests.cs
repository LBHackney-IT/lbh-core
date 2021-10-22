using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Hackney.Core.Validation.AspNet;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Hackney.Core.Tests.Validation.AspNet
{
    public class ValidationFailedExtensionsTests
    {
        private static Fixture _fixture = new Fixture();

        private static ValidationResult BuildValidationResult()
        {
            return _fixture.Create<ValidationResult>();
        }

        private static void ValidateErrors(IEnumerable<ValidationFailure> errors, ValidationFailedResponse responseObject)
        {
            var expected = new Dictionary<string, List<string>>();
            foreach (var error in errors)
            {
                if (!expected.ContainsKey(error.PropertyName)) expected.Add(error.PropertyName, new List<string>());
                var errorObject = new
                {
                    ErrorCode = error.ErrorCode,
                    ErrorMessage = error.ErrorMessage,
                    CustomState = error.CustomState
                };
                expected[error.PropertyName].Add(JsonSerializer.Serialize(errorObject));
            }

            responseObject.Errors.Should().BeEquivalentTo(expected);
        }


        [Fact]
        public void ValidationExceptionConstructResponseTestNullInputThrows()
        {
            ValidationException err = null;
            Action act = () => ValidationFailedExtensions.ConstructResponse(err);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Some error")]
        public void ValidationExceptionConstructResponseTestReturnsObject(string message)
        {
            ValidationResult result = BuildValidationResult();
            var exception = new ValidationException(message, result.Errors);
            var responseObject = exception.ConstructResponse();

            responseObject.Should().NotBeNull();
            responseObject.Message.Should().Be(exception.Message);
            responseObject.Status.Should().Be(400);
            ValidateErrors(exception.Errors, responseObject);
        }


        [Fact]
        public void ValidationResultConstructResponseTestNullInputThrows()
        {
            ValidationResult result = null;
            Action act = () => ValidationFailedExtensions.ConstructResponse(result);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidationResultConstructResponseTestReturnsObject()
        {
            ValidationResult result = BuildValidationResult();
            var responseObject = result.ConstructResponse();

            responseObject.Should().NotBeNull();
            responseObject.Message.Should().BeNull();
            responseObject.Status.Should().Be(400);
            ValidateErrors(result.Errors, responseObject);
        }
    }
}
