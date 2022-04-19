using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Hackney.Core.Validation.AspNet;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Hackney.Core.Tests.Validation.AspNet
{
    public class UseErrorCodeInterceptorTests
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly UseErrorCodeInterceptor _sut;
        private readonly ActionContext _actionContext;
        private readonly Mock<IValidationContext> _mockValidationContext;

        public UseErrorCodeInterceptorTests()
        {
            _jsonOptions = CreateJsonOptions();
            _sut = new UseErrorCodeInterceptor();

            _actionContext = new ActionContext();
            _mockValidationContext = new Mock<IValidationContext>();
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

        private static IEnumerable<ValidationFailure> ConstructFailures()
        {
            var failures = new List<ValidationFailure>();
            for (int i = 1; i < 5; i++)
            {
                failures.Add(new ValidationFailure($"prop-{i}", "Some error message")
                {
                    ErrorCode = i.ToString(),
                    CustomState = "some custom state"
                });
            }
            return failures;
        }

        [Fact]
        public void BeforeAspNetValidationTestReturnContext()
        {
            _sut.BeforeAspNetValidation(_actionContext, _mockValidationContext.Object)
                .Should().Be(_mockValidationContext.Object);
        }

        [Fact]
        public void AfterAspNetValidationTestNoErrorsReturnsResult()
        {
            var validationResult = new ValidationResult();

            _sut.AfterAspNetValidation(_actionContext, _mockValidationContext.Object, validationResult)
                .Should().Be(validationResult);
        }

        [Fact]
        public void AfterAspNetValidationTestReturnsProjectedResult()
        {
            var errors = ConstructFailures();
            var validationResult = new ValidationResult(errors);

            var projected = _sut.AfterAspNetValidation(_actionContext, _mockValidationContext.Object, validationResult);

            foreach (var p in projected.Errors)
            {
                var src = errors.FirstOrDefault(x => x.PropertyName == p.PropertyName);
                src.Should().NotBeNull();
                p.Should().BeEquivalentTo(src, config => config.Excluding(x => x.ErrorMessage));

                var expectedMsg = JsonSerializer.Serialize(new
                {
                    ErrorCode = src.ErrorCode,
                    ErrorMessage = "Some error message",
                    CustomState = src.CustomState
                }, _jsonOptions);
                p.ErrorMessage.Should().Be(expectedMsg);
            }

        }
    }
}