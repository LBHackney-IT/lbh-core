using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

/// <summary>
/// Controller is part of mock API for E2E tests. It is decorated with authorization attribute
/// to test if an undecorated endpoint method inherits it, and whether a decorated endpoint's
/// attribute overrides controller's attribute.
/// </summary>
[ApiController]
[Route(RoutePrefix)]
[AllowAccessTo(Machine = MachineAccess)]
public sealed class BotanicalController : WildlifeApiControllerBase
{
    public const string RoutePrefix = "botanical";
    public const string MachineAccess = "botanical";

    public const string ChamomileEndpointName = "chamomile";
    public const string DandelionEndpointName = "dandelion";

    // tested endpoints:

    [AllowAccessTo(Machine = ChamomileEndpointName)]
    [HttpGet(ChamomileEndpointName)]
    public IActionResult Chamomile() => Allowed();

    [HttpGet(DandelionEndpointName)]
    public IActionResult Dandelion() => Allowed();
}
