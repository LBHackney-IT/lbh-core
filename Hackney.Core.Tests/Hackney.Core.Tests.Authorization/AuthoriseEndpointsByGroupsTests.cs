using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.Authorization;
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Hackney.Core.Tests.Authorization;

public class AuthoriseEndpointByGroupsTests
{
    private const string PermittedGroupsVariable = "groups";
    private const string MissingAuthorizationHeaderMessage = "Missing or empty Authorization header value.";
    private const string UnknownTokenTypeMessage = "Unknown token type has been encountered. Access denied!";

    private readonly TokenGroupsFilter _classUnderTest;
    private readonly Mock<ITokenFactory> _mockTokenFactory;
    private readonly string[] _requiredGoogleGroups = ["test_group_name", "some-other-group"];
    private readonly Fixture _fixture = new();
    private AuthorizationFilterContext _context;
    private HeaderDictionary _requestHeaders;

    public AuthoriseEndpointByGroupsTests()
    {
        _mockTokenFactory = new Mock<ITokenFactory>();

        Environment.SetEnvironmentVariable(PermittedGroupsVariable, string.Join(",", _requiredGoogleGroups));

        _classUnderTest = new TokenGroupsFilter(_mockTokenFactory.Object, PermittedGroupsVariable);

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

    private static string UserUnauthorizedMessage(string? name) =>
        $"User {name} is not authorized to access this endpoint.";

    [Fact]
    public void Constructor_ThrowsEnvironmentVariableNullException_WhenPermittedGroupsVariableIsNotSet()
    {
        // Arrange
        var incorrectEnvVariable = "var";

        // Act
        Func<TokenGroupsFilter> func = () => new TokenGroupsFilter(_mockTokenFactory.Object, incorrectEnvVariable);

        // Assert
        func.Should().Throw<EnvironmentVariableNullException>()
            .WithMessage($"Cannot resolve {incorrectEnvVariable} environment variable.");
    }

    [Fact]
    public void Constructor_PassesPermittedGroupsVariableToTheGroupsFilter()
    {
        // Arrange
        const string permittedGroupsVariable = "ALLOWED_GROUPS";

        // Act
        var attribute = new AuthorizeEndpointByGroups(permittedGroupsVariable);

        // Assert
        attribute.ImplementationType.Should().Be(typeof(TokenGroupsFilter));
        attribute.Arguments.Should().ContainSingle().Which.Should().Be(permittedGroupsVariable);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void OnAuthorization_ReturnsUnauthorized_WhenAuthorizationHeaderIsMissingOrBlank(string? authorizationHeader)
    {
        // Arrange
        if (authorizationHeader is null)
            _requestHeaders.Remove("Authorization");
        else
            _requestHeaders["Authorization"] = authorizationHeader;

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be(MissingAuthorizationHeaderMessage);
        _mockTokenFactory.Verify(x => x.IdentifyHackneyToken(It.IsAny<string>()), Times.Never);
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenUserTokenCannotBeDecoded()
    {
        // Arrange
        const string token = "abc";
        _mockTokenFactory.Setup(x => x.IdentifyHackneyToken(token)).Returns(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(token)).Returns((Token?)null);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be(UserUnauthorizedMessage(null));
        _mockTokenFactory.Verify(x => x.IdentifyHackneyToken(token), Times.Once);
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(token), Times.Once);
    }

    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenUserTokenHasNoGroups()
    {
        // Arrange
        const string token = "abc";
        var userToken = _fixture.Build<Token>().With(x => x.Groups, []).Create();
        _mockTokenFactory.Setup(x => x.IdentifyHackneyToken(token)).Returns(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(token)).Returns(userToken);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be(UserUnauthorizedMessage(userToken.Name));
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(token), Times.Once);
    }

    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenUserTokenDoesNotContainRequiredGoogleGroup()
    {
        // Arrange
        const string token = "abc";
        var userToken = _fixture.Build<Token>().With(x => x.Groups, ["not-allowed", "also-not-allowed"]).Create();
        _mockTokenFactory.Setup(x => x.IdentifyHackneyToken(token)).Returns(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(token)).Returns(userToken);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be(UserUnauthorizedMessage(userToken.Name));
        _mockTokenFactory.Verify(x => x.IdentifyHackneyToken(token), Times.Once);
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(token), Times.Once);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenUserTokenContainsRequiredGoogleGroup()
    {
        // Arrange
        const string token = "user-token";
        _requestHeaders["Authorization"] = $"  {token}  ";
        var userToken = _fixture.Build<Token>()
            .With(x => x.Groups, ["not-allowed", "some-other-group"])
            .Create();
        _mockTokenFactory.Setup(x => x.IdentifyHackneyToken(token)).Returns(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(token)).Returns(userToken);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeNull();
        _mockTokenFactory.Verify(x => x.IdentifyHackneyToken(token), Times.Once);
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(token), Times.Once);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenTokenIsLegacyMachine()
    {
        // Arrange
        const string token = "legacy-machine-token";
        _requestHeaders["Authorization"] = $" Bearer {token} ";
        _mockTokenFactory.Setup(x => x.IdentifyHackneyToken($"Bearer {token}")).Returns(HackneyTokenType.MachineLegacy);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeNull();
        _mockTokenFactory.Verify(x => x.IdentifyHackneyToken($"Bearer {token}"), Times.Once);
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenTokenTypeIsUnknown()
    {
        // Arrange
        const string token = "abc";
        _mockTokenFactory.Setup(x => x.IdentifyHackneyToken(token)).Returns(HackneyTokenType.Unknown);

        // Act
        _classUnderTest.OnAuthorization(_context);

        // Assert
        _context.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be(UnknownTokenTypeMessage);
        _mockTokenFactory.Verify(x => x.IdentifyHackneyToken(token), Times.Once);
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(It.IsAny<string>()), Times.Never);
    }
}
