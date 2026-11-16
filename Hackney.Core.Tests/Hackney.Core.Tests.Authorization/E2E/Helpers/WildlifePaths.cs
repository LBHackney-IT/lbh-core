using Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

/// <summary>
/// Paths helper for the mock Wildlife API hosted by <see cref="WildlifeApiFixture"/>.
/// </summary>
internal static class WildlifePaths
{
    public static string BarbastellusBat =>
        $"{AnimalsController.RoutePrefix}/{AnimalsController.BarbastellusBatEndpointName}";

    public static string AquaticPath(string endpoint) =>
        $"{AquaticController.RoutePrefix}/{endpoint}";

    public static string BotanicalPath(string endpoint) =>
        $"{BotanicalController.RoutePrefix}/{endpoint}";

    public static string AtlanticSalmon =>
        AquaticPath(AquaticController.AtlanticSalmonEndpointName);
}
