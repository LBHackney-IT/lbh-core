using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using System;
using Xunit;

namespace Hackney.Core.Tests.Authorization
{
    public class ApplicationBuilderExtensionTests
    {
        [Fact]
        public void UseGoogleGroupAuthorizationTestNullAppThrows()
        {
            IApplicationBuilder app = null;
            
            Action act = () => Hackney.Core.Authorization.ApplicationBuilderExtension.UseGoogleGroupAuthorization(app);
            
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'IApplicationBuilder')");
        }
    }
}
