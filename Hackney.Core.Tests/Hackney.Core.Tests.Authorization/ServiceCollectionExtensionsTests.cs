using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using AwesomeAssertions;
using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Hackney.Core.Tests.Authorization;

public class ServiceCollectionExtensionsTests
{
    private const string ParameterName = "/auth/groups";

    [Fact]
    public async Task FallbackPolicy_DeniesAnAuthenticatedCaller()
    {
        await using var provider = await BuildProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var fallback = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;

        var caller = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "caller")],
            authenticationType: "Bearer"));

        var result = await authorization.AuthorizeAsync(caller, fallback!);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task FallbackPolicy_DeniesAnAnonymousCaller()
    {
        await using var provider = await BuildProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var fallback = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;

        var result = await authorization.AuthorizeAsync(new ClaimsPrincipal(), fallback!);

        result.Succeeded.Should().BeFalse();
    }

    private static async Task<ServiceProvider> BuildProvider()
    {
        var payload = JsonSerializer.Serialize(new Dictionary<string, string[]>
        {
            ["housing-officers"] = ["group-a"]
        });

        var ssm = new Mock<IAmazonSimpleSystemsManagement>();
        ssm.Setup(client => client.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse
            {
                Parameter = new Parameter { Value = payload }
            });

        var services = new ServiceCollection();
        await services.AddHackneyAuthorizationAsync(ParameterName, ssm.Object);
        return services.BuildServiceProvider();
    }
}
