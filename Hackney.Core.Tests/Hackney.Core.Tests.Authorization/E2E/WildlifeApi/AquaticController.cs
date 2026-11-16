using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

/// <summary>
/// Aquatic part of the mock Wildlife API. It is not a real service - it's a Controller used to
/// test the undecorated controller behaviour end to end. A decorated action allows access only
/// to itself, and an undecorated action is denied.
/// </summary>
[ApiController]
[Route(RoutePrefix)]
public sealed class AquaticController : WildlifeApiControllerBase
{
    public const string RoutePrefix = "aquatic";
    public const string AtlanticSalmonEndpointName = "atlantic-salmon";
    public const string GreatCrestedNewtEndpointName = "great-crested-newt";

    // tested endpoints:

    // for the sake of simplicity the machine scope endpoint name and endpoint url match,
    // however, in the real world scenario it does not need to be the case as the goal is
    // to have simple scope names that are not burdained by rest-style parameterised paths.
    [AllowAccessTo(Machine = AtlanticSalmonEndpointName)]
    [HttpGet(AtlanticSalmonEndpointName)]
    public IActionResult AtlanticSalmon() => Allowed();

    [HttpGet(GreatCrestedNewtEndpointName)]
    public IActionResult GreatCrestedNewt() => Allowed();
}
