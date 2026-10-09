using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using System;
using Xunit;
using Hackney.Core.Authorization;

namespace Hackney.Core.Tests.Authorization;

public class ApplicationBuilderExtensionTests
{
    [Fact]
    public void UseGoogleGroupAuthorizationTestNullAppThrows()
    {
        IApplicationBuilder? app = null;

        Action act = () => app.UseGoogleGroupAuthorization();

        act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'IApplicationBuilder')");
    }
}
