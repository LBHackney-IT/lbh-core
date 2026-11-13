using System;
using System.Net.Http;
using System.Threading.Tasks;
using Hackney.Core.Authorization;
using Hackney.Core.Tests.Authorization.E2E.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting; // unused?
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

/// <summary>
/// Starts the mock Wildlife API in memory for the tests in its collection. The Wildlife API is a
/// mock API used only to run the authorization functionality tests against.
/// </summary>
public sealed class WildlifeApiFixture : IAsyncLifetime
{
    private const string ParameterName = "/auth/groups";

    private WebApplication? _app;

    public HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(WildlifeApiFixture).Assembly.GetName().Name,
            ContentRootPath = AppContext.BaseDirectory,
            EnvironmentName = "Development"
        });

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(WildlifeApiFixture).Assembly);

        // The user flow looks up AnimalsController.UserAccess in this document. Cognito and legacy flows do not read it.
        var groups = $"{{\"{AnimalsController.UserAccess}\":[\"{AnimalsController.UserGroup}\"]}}";
        await builder.Services.AddHackneyAuthorizationAsync(ParameterName, FakeSsm.WithPayload(groups));

        _app = builder.Build();
        _app.UseAuthorization();
        _app.MapControllers();

        await _app.StartAsync();
        Client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        if (_app is not null)
            await _app.DisposeAsync();
    }
}
