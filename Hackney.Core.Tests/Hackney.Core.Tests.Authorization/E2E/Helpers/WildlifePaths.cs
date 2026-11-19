using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

/// <summary>
/// Paths helper for the mock Wildlife API hosted by <see cref="WildlifeApiFixture"/>.
/// </summary>
internal static class WildlifePaths
{
    public static string BarbastellusBat =>
        $"{AnimalsController.RoutePrefix}/{AnimalsController.BarbastellusBatEndpointName}";

    public static string MelesBadger =>
        $"{AnimalsController.RoutePrefix}/{AnimalsController.MelesBadgerEndpointName}";

    public static string AtlanticSalmon =>
        $"{AquaticController.RoutePrefix}/{AquaticController.AtlanticSalmonEndpointName}";

    public static string GreatCrestedNewt =>
        $"{AquaticController.RoutePrefix}/{AquaticController.GreatCrestedNewtEndpointName}";

    public static string LampetraLamprey =>
        $"{AquaticController.RoutePrefix}/{AquaticController.LampetraLampreyEndpointName}";

    public static string Chamomile =>
        $"{BotanicalController.RoutePrefix}/{BotanicalController.ChamomileEndpointName}";

    public static string Dandelion =>
        $"{BotanicalController.RoutePrefix}/{BotanicalController.DandelionEndpointName}";

    public static string BellisDaisy =>
        $"{BotanicalController.RoutePrefix}/{BotanicalController.BellisDaisyEndpointName}";
}
