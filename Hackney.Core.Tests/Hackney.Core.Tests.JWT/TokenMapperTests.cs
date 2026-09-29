using FluentAssertions;
using Hackney.Core.JWT;
using Xunit;

namespace Hackney.Core.Tests.JWT;

public class TokenMapperTests
{
    [Fact]
    public void MapStandardToken_MapsGroupsClaimWhenCustomGroupsAreAbsent()
    {
        // arrange
        var groups = new[] { "slifer-red" };

        var subject = "jaden-yuki";
        var email = "jaden.yuki@duel-academy.test";
        var name = "Jaden Yuki";

        const long notBefore = 100;
        const long expiresAt = 200;
        const long issuedAt = 50;

        var presentation = new TokenPresentation
        {
            Sub = subject,
            Groups = groups,
            CustomGroups = null,
            Email = email,
            Name = name,
            Nbf = notBefore,
            Exp = expiresAt,
            Iat = issuedAt
        };

        // act
        var token = TokenMapper.MapStandardToken(presentation);

        // assert
        token.Sub.Should().Be(subject);
        token.Groups.Should().Equal(groups);
        token.Email.Should().Be(email);
        token.Name.Should().Be(name);
        token.Nbf.Should().Be(notBefore);
        token.Exp.Should().Be(expiresAt);
        token.Iat.Should().Be(issuedAt);
    }

    [Fact]
    public void MapStandardToken_SplitsCustomGroupsAndOmitsEmptyEntries_WhenGroupsIsMissing()
    {
        // arrange
        var groupOne = "professors";
        var groupTwo = "slifer-red";
        var customGroups = $"{groupOne};;{groupTwo};";
        var presentation = new TokenPresentation
        {
            Sub = "lyman-banner",
            Groups = null,
            CustomGroups = customGroups,
            Email = "lyman.banner@duel-academy.test",
            Name = "Professor Lyman Banner",
            Iat = 50
        };

        // act
        var token = TokenMapper.MapStandardToken(presentation);

        // assert
        token.Groups.Should().Equal(groupOne, groupTwo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void MapStandardToken_ReturnsEmptyGroupsWhenCustomGroupsAreBlank(string? customGroups)
    {
        // arrange
        var presentation = new TokenPresentation
        {
            Sub = "jaden-yuki",
            Groups = null,
            CustomGroups = customGroups,
            Email = "jaden.yuki@duel-academy.test",
            Name = "Jaden Yuki",
            Iat = 50
        };

        // act
        var token = TokenMapper.MapStandardToken(presentation);

        // assert
        token.Groups.Should().BeEmpty();
    }

    [Fact]
    public void MapCognitoM2MToken_MapsValidScopes()
    {
        // arrange
        var clientId = "chazz-princeton-client";

        var sliferScope = new CognitoM2MAccessScope
        {
            ApiGatewayId = "duelacademyapi",
            EndpointName = "slifer-dorm",
            AccessType = "post"
        };

        var obelistScope = new CognitoM2MAccessScope
        {
            ApiGatewayId = "duelacademyapi",
            EndpointName = "obelisk-dorm",
            AccessType = "get"
        };

        string sliferScopeStr = $"{sliferScope.ApiGatewayId}/{sliferScope.EndpointName}.{sliferScope.AccessType}";
        string obelistScopeStr = $"{obelistScope.ApiGatewayId}/{obelistScope.EndpointName}.{obelistScope.AccessType}";
        var validScopes = $"{sliferScopeStr} {obelistScopeStr}";

        var presentation = new CognitoM2MTokenPresentation
        {
            ClientId = clientId,
            Scope = validScopes
        };

        // act
        var token = TokenMapper.MapCognitoM2MToken(presentation);

        // assert
        token.ClientId.Should().Be(clientId);
        token.Scopes.Should().BeEquivalentTo(sliferScope, obelistScope);
    }

    [Fact]
    public void MapCognitoM2MToken_IgnoresLeadingAndTrailingSpacesAroundScopes()
    {
        // arrange
        var expectedScope = new CognitoM2MAccessScope
        {
            ApiGatewayId = "duelacademyapi",
            EndpointName = "slifer-dorm",
            AccessType = "post"
        };

        var scope = $"{expectedScope.ApiGatewayId}/{expectedScope.EndpointName}.{expectedScope.AccessType}";
        string paddedScope = $" {scope} "; // whitespace at the start and end of scope str that needs trimming

        var presentation = new CognitoM2MTokenPresentation
        {
            ClientId = "jaden-yuki-client",
            Scope = paddedScope
        };

        // act
        var token = TokenMapper.MapCognitoM2MToken(presentation);

        // assert
        token.Scopes.Should().BeEquivalentTo(expectedScope);
    }

    [Theory]
    [InlineData("duelacademyapi/slifer_dorm.post")]
    [InlineData("professorbanner.post")]
    [InlineData("duelacademyapi/slifer-dorm/wingeddragon.get")]
    public void MapCognitoM2MToken_IgnoresMalformedScopeValue(string scope)
    {
        // arrange
        var presentation = new CognitoM2MTokenPresentation
        {
            ClientId = "professor-banner-client",
            Scope = scope
        };

        // act
        var token = TokenMapper.MapCognitoM2MToken(presentation);

        // assert
        token.Scopes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("/slifer-dorm.post")]
    [InlineData("duelacademyapi/.post")]
    [InlineData("duelacademyapi/slifer-dorm.")]
    public void MapCognitoM2MToken_IgnoresScopeWithMissingComponent(string scope)
    {
        // arrange
        var presentation = new CognitoM2MTokenPresentation
        {
            ClientId = "professor-banner-client",
            Scope = scope
        };

        // act
        var token = TokenMapper.MapCognitoM2MToken(presentation);

        // assert
        token.Scopes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void MapCognitoM2MToken_ReturnsEmptyScopesWhenScopeClaimIsBlank(string? scope)
    {
        // arrange
        var clientId = "chazz-princeton-client";
        var presentation = new CognitoM2MTokenPresentation
        {
            ClientId = clientId,
            Scope = scope
        };

        // act
        var token = TokenMapper.MapCognitoM2MToken(presentation);

        // assert
        token.ClientId.Should().Be(clientId);
        token.Scopes.Should().BeEmpty();
    }
}