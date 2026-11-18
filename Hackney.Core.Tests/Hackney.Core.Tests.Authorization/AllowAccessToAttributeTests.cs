using System;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Hackney.Core.Tests.Authorization;

public class AllowAccessToAttributeTests
{
    [Fact]
    public void Attribute_ExposesTheFixedHybridAccessPolicy()
    {
        // act
        var attribute = new AllowAccessToAttribute();

        // assert
        attribute.Should().BeAssignableTo<IAuthorizeData>();

        // sealed so devs can't inherit from it & accidentally override policy that way
        typeof(AllowAccessToAttribute).IsSealed.Should().BeTrue();

        attribute.Policy.Should().Be(AllowAccessToAttribute.PolicyName);
        attribute.User.Should().BeNull();
        attribute.Machine.Should().BeNull();
        attribute.LegacyMachine.Should().BeFalse();
    }

    // ensuring it's a custom implementation of 'IAuthorizeData' rather than descendent of
    // 'AuthorizeAttribute' as it was before so the 'Roles', 'AuthenticationSchemes', and
    // 'Policy' keys are locked down to prevent developers from shooting themselves in the foot
    [Fact]
    public void PolicyRolesAndSchemes_StayFixedWhenSetThroughTheInterface()
    {
        // arrange
        IAuthorizeData attribute = new AllowAccessToAttribute();

        // act
        attribute.Policy = "Other";
        attribute.Roles = "Admin";
        attribute.AuthenticationSchemes = "Bearer";

        // assert
        attribute.Policy.Should().Be(AllowAccessToAttribute.PolicyName);
        attribute.Roles.Should().BeNull();
        attribute.AuthenticationSchemes.Should().BeNull();
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
            Machine = machine,
            LegacyMachine = true
        };

        // assert
        attribute.User.Should().Be(user);
        attribute.Machine.Should().Be(machine);
        attribute.LegacyMachine.Should().BeTrue();
        attribute.Policy.Should().Be(AllowAccessToAttribute.PolicyName);
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
        attribute!.Policy.Should().Be(AllowAccessToAttribute.PolicyName);
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
        attribute!.Policy.Should().Be(AllowAccessToAttribute.PolicyName);
        attribute.User.Should().Be("method-users");
        attribute.Machine.Should().Be("method-clients");
    }

    [Fact]
    public void UndecoratedEndpoint_ReturnsControllerLevelUserAndMachine()
    {
        // act
        var attribute = EndpointAccessFor(nameof(MockController.List));

        // assert
        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(AllowAccessToAttribute.PolicyName);
        attribute.User.Should().Be("class-users");
        attribute.Machine.Should().Be("class-clients");
    }

    private static AllowAccessToAttribute? EndpointAccessFor(string actionName)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(AllowAccessToAttributeTests).Assembly.GetName().Name,
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.Logging.ClearProviders();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(AllowAccessToAttributeTests).Assembly);

        using var app = builder.Build();
        var provider = app.Services.GetRequiredService<IActionDescriptorCollectionProvider>();
        var descriptor = provider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Single(action => action.ControllerTypeInfo == typeof(MockController).GetTypeInfo()
                && action.MethodInfo.Name == actionName);

        var metadata = new EndpointMetadataCollection(descriptor.EndpointMetadata);

        return metadata.GetMetadata<AllowAccessToAttribute>();
    }
}

// MVC only discovers public top-level classes as controllers. A nested class is
// ignored, so the class-level attribute would never be copied onto an endpoint.
[AllowAccessTo(User = "class-users", Machine = "class-clients")]
[Route("mock")]
public sealed class MockController
{
    [AllowAccessTo(User = "method-users", Machine = "method-clients")]
    [HttpGet("item")]
    public void Get()
    {
    }

    [HttpGet("list")]
    public void List()
    {
    }
}
