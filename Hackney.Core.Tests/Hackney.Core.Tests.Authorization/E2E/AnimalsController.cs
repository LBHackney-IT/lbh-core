using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hackney.Core.Tests.Authorization.E2E;

/// <summary>
/// Controller of the mock Wildlife API. It exists only so the authorization tests have endpoints
/// to call for different token authorization flow testing.
/// </summary>
[ApiController]
[Route(RoutePrefix)]
public sealed class AnimalsController : ControllerBase
{
    public const string RoutePrefix = "animals";
    public const string BarbastellusBatEndpointName = "barbastellus-bat";

    // tested endpoints:

    [AllowAccessTo(Machine = BarbastellusBatEndpointName)]
    [HttpGet("barbastellus-bat")]
    public IActionResult BarbastellusBat() => Ok(new { status = "allowed" });
}
