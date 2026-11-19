using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Hackney.Core.Authorization;
using Hackney.Core.Authorization.Internal;
using Hackney.Core.JWT;
using Hackney.Core.Tests.Authorization.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hackney.Core.Tests.Authorization.Internal;

public class HybridAccessHandlerTests
{
    private const string UserPolicyKey = "housing-officers";
    private const string MachineEndpoint = "tunnel";
    private const string AllowedGroup = "allowed-group";
    private const string RawJwt = "header.payload.signature";

    private readonly Mock<ITokenFactory> _mockTokenFactory = new();
    private readonly CapturingLogger<HybridAccessHandler> _logger = new();
    private readonly HybridAccessRequirement _requirement = new();
    private readonly PolicyGroupMap _policyMap = new(new Dictionary<string, HashSet<string>>
    {
        [UserPolicyKey] = [AllowedGroup, "another-allowed-group"]
    });
    private readonly HybridAccessHandler _classUnderTest;

    public HybridAccessHandlerTests()
    {
        _classUnderTest = new HybridAccessHandler(_policyMap, _logger);
    }

    #region helper methods

    private static AllowAccessToAttribute Rule(
        string? user = UserPolicyKey,
        string? machine = MachineEndpoint,
        bool legacyMachine = false) =>
        new() { User = user, Machine = machine, LegacyMachine = legacyMachine };

    private HttpContext BuildHttpContext(
        AllowAccessToAttribute? rule,
        string? authorizationHeader = RawJwt,
        bool registerTokenFactory = true)
    {
        var services = new ServiceCollection();
        if (registerTokenFactory) services.AddSingleton(_mockTokenFactory.Object);

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        if (authorizationHeader != null) httpContext.Request.Headers.Authorization = authorizationHeader;

        var metadata = rule == null ? new EndpointMetadataCollection() : new EndpointMetadataCollection(rule);
        httpContext.SetEndpoint(new Endpoint(null, metadata, "test-endpoint"));

        return httpContext;
    }

    private AuthorizationHandlerContext BuildAuthContext(object? resource) =>
        new([_requirement], new ClaimsPrincipal(new ClaimsIdentity()), resource);

    private async Task<AuthorizationHandlerContext> HandleAsync(HttpContext httpContext)
    {
        var context = BuildAuthContext(httpContext);
        await _classUnderTest.HandleAsync(context);
        return context;
    }

    private void SetupTokenType(HackneyTokenType type, string jwt = RawJwt) =>
        _mockTokenFactory.Setup(x => x.IdentifyHackneyToken(jwt)).Returns(type);

    private static Token UserToken(params string[] groups) => new()
    {
        Sub = "sub",
        Email = "user@example.com",
        Name = "Test User",
        Groups = groups,
        Iat = 1
    };

    private static CognitoM2MToken CognitoToken(params (string endpoint, string access)[] scopes) => new()
    {
        ClientId = "client-123",
        Scopes = scopes.Select(s => new CognitoM2MAccessScope
        {
            LogicalApiName = "wildlife-api",
            EndpointName = s.endpoint,
            AccessType = s.access
        }).ToList()
    };

    private static LegacyMachineToken LegacyToken() => new()
    {
        Id = "42",
        ConsumerName = "some-consumer",
        ConsumerType = "Service",
        Iat = 1
    };

    #endregion

    #region constructor

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPolicyMapIsNull()
    {
        // act
        Func<HybridAccessHandler> act = () => new HybridAccessHandler(null!, _logger);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("policyMap");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // act
        Func<HybridAccessHandler> act = () => new HybridAccessHandler(_policyMap, null!);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    #endregion

    #region pre-token checks

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenResourceIsNull()
    {
        // act
        var context = BuildAuthContext(null);
        await _classUnderTest.HandleAsync(context);

        // assert
        context.HasSucceeded.Should().BeFalse();
        _mockTokenFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenResourceIsNotHttpContext()
    {
        // act
        var context = BuildAuthContext("not-an-http-context");
        await _classUnderTest.HandleAsync(context);

        // assert
        context.HasSucceeded.Should().BeFalse();
        _mockTokenFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenEndpointHasNoAllowAccessToAttribute()
    {
        // arrange
        var httpContext = BuildHttpContext(rule: null);

        // act
        var context = await HandleAsync(httpContext);

        // assert
        context.HasSucceeded.Should().BeFalse();
        _mockTokenFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenThereIsNoEndpoint()
    {
        // arrange
        var httpContext = new DefaultHttpContext();

        // act
        var context = await HandleAsync(httpContext);

        // assert
        context.HasSucceeded.Should().BeFalse();
        _mockTokenFactory.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_DeniesAndWarns_WhenAuthorizationHeaderIsMissingOrBlank(string? header)
    {
        // arrange
        var httpContext = BuildHttpContext(Rule(), header);

        // act
        var context = await HandleAsync(httpContext);

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "missing or empty Authorization header");
        _mockTokenFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_DeniesAndLogsError_WhenTokenFactoryIsNotRegistered()
    {
        // arrange
        var httpContext = BuildHttpContext(Rule(), registerTokenFactory: false);

        // act
        var context = await HandleAsync(httpContext);

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Error, nameof(ITokenFactory));
    }

    [Fact]
    public async Task HandleAsync_PassesTrimmedAuthorizationHeaderToTheTokenFactory()
    {
        // arrange
        var httpContext = BuildHttpContext(Rule(), $"  Bearer {RawJwt}  ");
        SetupTokenType(HackneyTokenType.Unknown, $"Bearer {RawJwt}");

        // act
        await HandleAsync(httpContext);

        // assert
        _mockTokenFactory.Verify(x => x.IdentifyHackneyToken($"Bearer {RawJwt}"), Times.Once);
    }

    #endregion

    #region unknown token type

    [Fact]
    public async Task HandleAsync_DeniesAndWarns_WhenTokenTypeIsUnknown()
    {
        // arrange
        SetupTokenType(HackneyTokenType.Unknown);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "unknown or unsupported token type");
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(It.IsAny<string>()), Times.Never);
        _mockTokenFactory.Verify(x => x.DecodeCognitoM2MToken(It.IsAny<string>()), Times.Never);
        _mockTokenFactory.Verify(x => x.Decode<LegacyMachineToken>(It.IsAny<string>()), Times.Never);
    }

    // Should a bug be introduced in JWT library that cause the Unknown type to not longer be
    // the catch-all fallback, this test guarantees that both Unknown and the actually unknown
    // enum value will be treated the same.
    [Fact]
    public async Task HandleAsync_DeniesAndWarns_WhenTokenTypeIsOutOfRange()
    {
        // arrange
        SetupTokenType((HackneyTokenType)999);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "unknown or unsupported token type");
    }

    #endregion

    #region empty attribute

    [Theory]
    [InlineData(HackneyTokenType.User, "does not allow user access")]
    [InlineData(HackneyTokenType.MachineLegacy, "does not allow legacy machine access")]
    [InlineData(HackneyTokenType.CognitoM2M, "does not allow machine access")]
    public async Task HandleAsync_DeniesAndWarns_WhenTheAttributeHasNoRules(
        HackneyTokenType tokenType,
        string warning)
    {
        // arrange
        SetupTokenType(tokenType);

        // act
        var context = await HandleAsync(BuildHttpContext(new AllowAccessToAttribute()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, warning);
    }

    #endregion

    #region user token

    [Fact]
    public async Task HandleAsync_Succeeds_WhenUserTokenHasAnAllowedGroup()
    {
        // arrange
        SetupTokenType(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(RawJwt))
            .Returns(UserToken("unrelated-group", AllowedGroup));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeTrue();
        _logger.Entries.Should().NotContain(e => e.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenUserTokenMatchesAndMachineIsNotSet()
    {
        // arrange
        SetupTokenType(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(RawJwt)).Returns(UserToken(AllowedGroup));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(machine: null)));

        // assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task HandleAsync_DeniesAndWarns_WhenUserTokenUsedButEndpointHasNoUserRule(string? user)
    {
        // arrange
        SetupTokenType(HackneyTokenType.User);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(user: user)));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "does not allow user access");
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DeniesAndWarns_WhenUserTokenCannotBeDecoded()
    {
        // arrange
        SetupTokenType(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(RawJwt)).Returns((Token?)null);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "user token could not be decoded");
    }

    [Fact]
    public async Task HandleAsync_DeniesAndWarns_WhenPolicyKeyIsNotInThePolicyMap()
    {
        // arrange
        SetupTokenType(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(RawJwt)).Returns(UserToken(AllowedGroup));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(user: "unknown-policy")));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "no group policy is configured");
        _logger.ShouldHaveLogged(LogLevel.Warning, "unknown-policy");
    }

    [Theory]
    [InlineData]
    [InlineData("other-group")]
    [InlineData("other-group", "yet-another-group")]
    public async Task HandleAsync_DeniesAndWarns_WhenUserTokenGroupsDoNotMatchPolicy(params string[] groups)
    {
        // arrange
        SetupTokenType(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(RawJwt)).Returns(UserToken(groups));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "groups do not match policy");
    }

    #endregion

    #region  legacy machine token

    [Fact]
    public async Task HandleAsync_DeniesAndWarns_WhenEndpointDoesNotAllowLegacyMachine()
    {
        // arrange
        SetupTokenType(HackneyTokenType.MachineLegacy);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "does not allow legacy machine access");
        _mockTokenFactory.Verify(x => x.Decode<LegacyMachineToken>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SucceedsAndLogsTokenId_WhenLegacyMachineTokenIsDecoded()
    {
        // arrange
        var token = LegacyToken();
        SetupTokenType(HackneyTokenType.MachineLegacy);
        _mockTokenFactory.Setup(x => x.Decode<LegacyMachineToken>(RawJwt)).Returns(token);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(legacyMachine: true)));

        // assert
        context.HasSucceeded.Should().BeTrue();
        _logger.ShouldHaveLogged(LogLevel.Information, token.Id);
        _logger.ShouldHaveLogged(LogLevel.Information, token.ConsumerName);
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenLegacyMachineTokenIsDecoded_AndEndpointHasNoUserOrMachineRule()
    {
        // arrange
        SetupTokenType(HackneyTokenType.MachineLegacy);
        _mockTokenFactory.Setup(x => x.Decode<LegacyMachineToken>(RawJwt)).Returns(LegacyToken());

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(user: null, machine: null, legacyMachine: true)));

        // assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_DeniesAndWarns_WhenLegacyMachineTokenCannotBeDecoded()
    {
        // arrange
        SetupTokenType(HackneyTokenType.MachineLegacy);
        _mockTokenFactory.Setup(x => x.Decode<LegacyMachineToken>(RawJwt)).Returns((LegacyMachineToken?)null);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(legacyMachine: true)));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "Id could not be determined");
    }

    #endregion

    #region cognito machine token

    [Fact]
    public async Task HandleAsync_Succeeds_WhenCognitoTokenHasMatchingEndpointScope()
    {
        // arrange
        SetupTokenType(HackneyTokenType.CognitoM2M);
        _mockTokenFactory.Setup(x => x.DecodeCognitoM2MToken(RawJwt))
            .Returns(CognitoToken(("other-endpoint", "get"), (MachineEndpoint, "post")));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeTrue();
        _logger.Entries.Should().NotContain(e => e.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenCognitoEndpointScopeDiffersOnlyByCase()
    {
        // arrange
        SetupTokenType(HackneyTokenType.CognitoM2M);
        _mockTokenFactory.Setup(x => x.DecodeCognitoM2MToken(RawJwt))
            .Returns(CognitoToken((MachineEndpoint.ToUpperInvariant(), "read")));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenCognitoTokenMatchesAndUserIsNotSet()
    {
        // arrange
        SetupTokenType(HackneyTokenType.CognitoM2M);
        _mockTokenFactory.Setup(x => x.DecodeCognitoM2MToken(RawJwt))
            .Returns(CognitoToken((MachineEndpoint, "read")));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(user: null)));

        // assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task HandleAsync_DeniesAndWarns_WhenCognitoTokenUsed_ButEndpointHasNoMachineRule(string? machine)
    {
        // arrange
        SetupTokenType(HackneyTokenType.CognitoM2M);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule(machine: machine)));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "does not allow machine access");
        _mockTokenFactory.Verify(x => x.DecodeCognitoM2MToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DeniesAndWarns_WhenCognitoTokenCannotBeDecoded()
    {
        // arrange
        SetupTokenType(HackneyTokenType.CognitoM2M);
        _mockTokenFactory.Setup(x => x.DecodeCognitoM2MToken(RawJwt)).Returns((CognitoM2MToken?)null);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "Cognito M2M token could not be decoded");
    }

    [Theory]
    [InlineData]
    [InlineData("other")]
    [InlineData("tunnels", "tunne")]
    public async Task HandleAsync_DeniesAndWarns_WhenCognitoTokenHasNoMatchingEndpointScope(params string[] endpoints)
    {
        // arrange
        SetupTokenType(HackneyTokenType.CognitoM2M);
        var token = CognitoToken(endpoints.Select(e => (e, "read")).ToArray());
        _mockTokenFactory.Setup(x => x.DecodeCognitoM2MToken(RawJwt)).Returns(token);

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _logger.ShouldHaveLogged(LogLevel.Warning, "has no scope for endpoint");
        _logger.ShouldHaveLogged(LogLevel.Warning, token.ClientId);
        _logger.ShouldHaveLogged(LogLevel.Warning, MachineEndpoint);
    }

    #endregion

    #region cross-flow isolation

    [Fact]
    public async Task HandleAsync_DoesNotUseMachineRule_ForUserTokens()
    {
        // arrange
        // the user is not in an allowed group, the machine rule must not rescue the request
        SetupTokenType(HackneyTokenType.User);
        _mockTokenFactory.Setup(x => x.DecodeStandardToken(RawJwt)).Returns(UserToken("other-group"));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _mockTokenFactory.Verify(x => x.DecodeCognitoM2MToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DoesNotUseUserRule_ForCognitoTokens()
    {
        // arrange: scopes do not match, the user rule must not rescue the request
        SetupTokenType(HackneyTokenType.CognitoM2M);
        _mockTokenFactory.Setup(x => x.DecodeCognitoM2MToken(RawJwt)).Returns(CognitoToken(("other", "read")));

        // act
        var context = await HandleAsync(BuildHttpContext(Rule()));

        // assert
        context.HasSucceeded.Should().BeFalse();
        _mockTokenFactory.Verify(x => x.DecodeStandardToken(It.IsAny<string>()), Times.Never);
    }

    #endregion
}
