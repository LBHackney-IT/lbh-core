using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Hackney.Core.Authorization.Internal;
using Xunit;

namespace Hackney.Core.Tests.Authorization.Internal;

public class PolicyGroupMapTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPoliciesIsNull()
    {
        // act
        Func<PolicyGroupMap> act = () => new PolicyGroupMap(null!);

        // assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("policies");
    }

    [Fact]
    public void Constructor_StoresTheProvidedPolicies()
    {
        // arrange
        var policies = new Dictionary<string, HashSet<string>>
        {
            ["HybridAccessPolicy"] = ["housing-officers", "service-client"],
            ["AnotherPolicy"] = ["finance-readers"]
        };

        // act
        var map = new PolicyGroupMap(policies);

        // assert
        map.Policies.Should().BeSameAs(policies);
        map.Policies["HybridAccessPolicy"].Should().BeEquivalentTo("housing-officers", "service-client");
        map.Policies["AnotherPolicy"].Should().BeEquivalentTo("finance-readers");
    }

    [Fact]
    public void Constructor_AllowsAnEmptyPolicyMap()
    {
        // arrange
        var policies = new Dictionary<string, HashSet<string>>();

        // act
        var map = new PolicyGroupMap(policies);

        // assert
        map.Policies.Should().BeEmpty();
        map.Policies.Should().BeSameAs(policies);
    }
}
