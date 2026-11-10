using System;
using System.Reflection;
using AwesomeAssertions;
using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Hackney.Core.Tests.Authorization;

public class AllowAccessToAttributeTests
{
    private const string HybridAccessPolicy = "HybridAccessPolicy";

    [Fact]
    public void Constructor_SetsHybridAccessPolicy()
    {
        // act
        var attribute = new AllowAccessToAttribute();

        // assert
        attribute.Should().BeAssignableTo<AuthorizeAttribute>();
        attribute.Policy.Should().Be(HybridAccessPolicy);
        attribute.User.Should().BeNull();
        attribute.Machine.Should().BeNull();
    }

    [Fact]
    public void Properties_RetainAssignedUserAndMachine()
    {
        // arrange
        const string user = "housing-officers";
        const string machine = "resource-server-scope";

        // act
        var attribute = new AllowAccessToAttribute
        {
            User = user,
            Machine = machine
        };

        // assert
        attribute.User.Should().Be(user);
        attribute.Machine.Should().Be(machine);
        attribute.Policy.Should().Be(HybridAccessPolicy);
    }

    [Fact]
    public void AttributeUsage_TargetsClassesAndMethodsAndDisallowsMultiple()
    {
        // act
        var usage = typeof(AllowAccessToAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        // assert
        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Class | AttributeTargets.Method);
        usage.AllowMultiple.Should().BeFalse();
        usage.Inherited.Should().BeTrue();
    }

    [Fact]
    public void AppliedToClass_ExposesConfiguredUserAndMachine()
    {
        // act
        var attribute = typeof(MockController).GetCustomAttribute<AllowAccessToAttribute>();

        // assert
        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(HybridAccessPolicy);
        attribute.User.Should().Be("class-users");
        attribute.Machine.Should().Be("class-clients");
    }

    // the method level attribute application overrides the class level application.
    // meaning, controller allows setting a base line, while method-level gives more
    // granular control that overrides whatever was set at a controller-level.
    [Fact]
    public void AppliedToMethod_ExposesConfiguredUserAndMachine()
    {
        // arrange
        var method = typeof(MockController).GetMethod(nameof(MockController.Get));

        // act
        var attribute = method!.GetCustomAttribute<AllowAccessToAttribute>();

        // assert
        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(HybridAccessPolicy);
        attribute.User.Should().Be("method-users");
        attribute.Machine.Should().Be("method-clients");
    }

    // example mock controller to which this tested attribute is applied at both
    // controller-level and the method-level with different input arguments.
    [AllowAccessTo(User = "class-users", Machine = "class-clients")]
    private sealed class MockController
    {
        [AllowAccessTo(User = "method-users", Machine = "method-clients")]
        public static void Get()
        {
        }
    }
}
