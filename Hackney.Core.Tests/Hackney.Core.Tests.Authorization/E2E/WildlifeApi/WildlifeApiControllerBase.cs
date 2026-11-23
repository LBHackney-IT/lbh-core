using Microsoft.AspNetCore.Mvc;

namespace Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

public abstract class WildlifeApiControllerBase : ControllerBase
{
    public const string AllowedStatus = "allowed";

    protected IActionResult Allowed() => Ok(new { status = AllowedStatus });
}
