using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using Hackney.Core.Authorization;
using FluentAssertions;
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Hackney.Core.Tests.Authorization;

public class AuthoriseEndpointByGroupsTests
{
    TokenGroupsFilter _classUnderTest;
    Mock<ITokenFactory> _mockTokenFactory;
    private readonly string[] _requiredGoogleGroups = ["test_group_name", "some-other-group"];
    private static Fixture _fixture => new Fixture();
    private AuthorizationFilterContext _context;
    private HeaderDictionary _requestHeaders;

    public AuthoriseEndpointByGroupsTests()
    {
        _mockTokenFactory = new Mock<ITokenFactory>();

        Environment.SetEnvironmentVariable("groups", string.Join(",", _requiredGoogleGroups));

        _classUnderTest = new TokenGroupsFilter(_mockTokenFactory.Object, "groups");

        SetUpMockContextAndHeaders();
    }

    [MemberNotNull(nameof(_context), nameof(_requestHeaders))]
    private void SetUpMockContextAndHeaders()
    {
        _requestHeaders = new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", "abc" } });

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.Headers).Returns(_requestHeaders);

        var actionContext = new ActionContext(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
        _context = new AuthorizationFilterContext(actionContext, []);
    }

    [Fact]
    public void ConstructorThrowsErrorIfGroupsEnvVariableIsNull()
    {
        var incorrectEnvVariable = "var";

        Func<TokenGroupsFilter> func = () => new TokenGroupsFilter(_mockTokenFactory.Object, incorrectEnvVariable);

        func.Should().Throw<EnvironmentVariableNullException>().WithMessage($"Cannot resolve {incorrectEnvVariable} environment variable.");
    }

    [Fact]
    public void OnAuthorizationResultIsUnauthorizedIfTokenIsNull()
    {
        // Arrange
        Token? missingToken = null;
        _mockTokenFactory.Setup(x => x.Create(_requestHeaders, "Authorization")).Returns(missingToken);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("User  is not authorized to access this endpoint.");
        _mockTokenFactory.Verify(x => x.Create(_requestHeaders, "Authorization"), Times.Once);
    }

    [Fact]
    public void OnAuthorizationResultIsUnauthorizedIfTokenDoesNotContainRequiredGoogleGroups()
    {
        // Arrange
        var userToken = _fixture.Build<Token>().With(x => x.Groups, ["not one of the allowed groups"]).Create();
        _mockTokenFactory.Setup(x => x.Create(_requestHeaders, "Authorization")).Returns(userToken);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be($"User {userToken.Name} is not authorized to access this endpoint.");
        _mockTokenFactory.Verify(x => x.Create(_requestHeaders, "Authorization"), Times.Once);
    }

    [Fact]
    public void OnAuthorizationResultIsNullIfTokenContainsOneOfTheRequiredGoogleGroups()
    {
        // Arrange
        var userToken = _fixture.Build<Token>().With(x => x.Groups, ["test_group_name"]).Create();
        _mockTokenFactory.Setup(x => x.Create(_requestHeaders, "Authorization")).Returns(userToken);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeNull();
        _mockTokenFactory.Verify(x => x.Create(_requestHeaders, "Authorization"), Times.Once);
    }
}
