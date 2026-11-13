using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hackney.Core.Tests.Authorization.E2E;

/// <summary>
/// Controller of the mock Wildlife API. It exists only so the authorization tests have endpoints
/// to call. It is not a real service and has no business logic.
/// </summary>
[ApiController]
[Route("machine-access")]
public sealed class WildlifeApiController : ControllerBase
{
    public const string BarbastellusBatEndpointName = "barbastellus-bat";

    [AllowAccessTo(Machine = BarbastellusBatEndpointName)]
    [HttpGet("barbastellus-bat")]
    public IActionResult BarbastellusBat() => Ok(new { status = "allowed" });
}
