using Xunit;

namespace Hackney.Core.Tests.Authorization.E2E.WildlifeApi;

/// <summary>
/// Test collection that shares one running <see cref="WildlifeApiFixture"/>, which hosts the
/// mock Wildlife API. The Wildlife API is a mock API used only to run the authorization
/// functionality tests against. It is not a real service and has no business logic.
/// </summary>
[CollectionDefinition(Name)]
public sealed class WildlifeApiCollection : ICollectionFixture<WildlifeApiFixture>
{
    public const string Name = "Wildlife API";
}
