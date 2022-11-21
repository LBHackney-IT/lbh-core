using System;
using System.Collections.Generic;
using AutoFixture;
using Hackney.Core.Authorization;
using FluentAssertions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Hackney.Core.Tests.Authorization
{
    public class AuthoriseEndpointByGroupsFacts
    {
        TokenGroupsFilter _classUnderFact;
        Mock<ITokenFactory> _mockTokenFactory;
        private string[] _requiredGoogleGroups;
        private static Fixture _fixture => new Fixture();

        public AuthoriseEndpointByGroupsFacts()
        {
            _mockTokenFactory = new Mock<ITokenFactory>();

            _requiredGoogleGroups = new string[] { "test_group_name", "some-other-group" };
            Environment.SetEnvironmentVariable("groups", string.Join(",", _requiredGoogleGroups));

            _classUnderFact = new TokenGroupsFilter(_mockTokenFactory.Object, "groups");
        }

        [Fact]
        public void ConstructorThrowsErrorIfGroupsEnvVariableIsNull()
        {
            var incorrectEnvVariable = "var";

            Func<TokenGroupsFilter> func = () => new TokenGroupsFilter(_mockTokenFactory.Object, incorrectEnvVariable);

            func.Should().Throw<EnvironmentVariableNullException>().WithMessage($"Cannot resolve {incorrectEnvVariable} environment variable.");
        }

        private (AuthorizationFilterContext, HeaderDictionary) SetUpMockContextAndHeaders()
        {
            var requestHeaders = new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", "abc" } });

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.Request.Headers).Returns(requestHeaders);

            var actionContext = new ActionContext(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            var context = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            return (context, requestHeaders);
        }

        [Fact]
        public void OnAuthorizationResultIsUnauthorizedIfTokenIsNull()
        {
            // Arrange
            (var context, var requestHeaders) = SetUpMockContextAndHeaders();
            _mockTokenFactory.Setup(x => x.Create(requestHeaders, "Authorization")).Returns((Token)null);
            // Act
            _classUnderFact.OnAuthorization(context);
            // Assert
            context.Result.Should().BeOfType(typeof(UnauthorizedObjectResult));
            (context.Result as UnauthorizedObjectResult).Value.Should().Be("User  is not authorized to access this endpoint.");
            _mockTokenFactory.Verify(x => x.Create(requestHeaders, "Authorization"), Times.Once);
        }

        [Fact]
        public void OnAuthorizationResultIsUnauthorizedIfTokenDoesNotContainRequiredGoogleGroups()
        {
            // Arrange
            (var context, var requestHeaders) = SetUpMockContextAndHeaders();
            var userToken = _fixture.Build<Token>().With(x => x.Groups, new string[] { "not one of the allowed groups" }).Create();
            _mockTokenFactory.Setup(x => x.Create(requestHeaders, "Authorization")).Returns((Token)userToken);
            // Act
            _classUnderFact.OnAuthorization(context);
            // Assert
            context.Result.Should().BeOfType(typeof(UnauthorizedObjectResult));
            (context.Result as UnauthorizedObjectResult).Value.Should().Be($"User {userToken.Name} is not authorized to access this endpoint.");
            _mockTokenFactory.Verify(x => x.Create(requestHeaders, "Authorization"), Times.Once);
        }

        [Fact]
        public void OnAuthorizationResultIsNullIfTokenContainsOneOfTheRequiredGoogleGroups()
        {
            // Arrange
            (var context, var requestHeaders) = SetUpMockContextAndHeaders();
            var userToken = _fixture.Build<Token>().With(x => x.Groups, new string[] { "test_group_name" }).Create();
            _mockTokenFactory.Setup(x => x.Create(requestHeaders, "Authorization")).Returns((Token)userToken);
            // Act
            _classUnderFact.OnAuthorization(context);
            // Assert
            context.Result.Should().BeNull();
            _mockTokenFactory.Verify(x => x.Create(requestHeaders, "Authorization"), Times.Once);
        }

    }
}
