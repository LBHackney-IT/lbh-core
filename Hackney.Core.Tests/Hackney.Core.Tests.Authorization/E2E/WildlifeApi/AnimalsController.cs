using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

/// <summary>
/// Controller of the mock Wildlife API. It exists only so the authorization tests have endpoints
/// to call for different token authorization flow testing.
/// </summary>
[ApiController]
[Route(RoutePrefix)]
public sealed class AnimalsController : WildlifeApiControllerBase
{
    public const string RoutePrefix = "animals";
    public const string BarbastellusBatEndpointName = "barbastellus-bat";
    public const string UserAccess = "wildlife-keepers";
    public const string UserGroup = "mammal-surveyors";

    // tested endpoints:

    [AllowAccessTo(Machine = BarbastellusBatEndpointName, User = UserAccess, LegacyMachine = true)]
    [HttpGet(BarbastellusBatEndpointName)]
    public IActionResult BarbastellusBat() => Allowed();
}
