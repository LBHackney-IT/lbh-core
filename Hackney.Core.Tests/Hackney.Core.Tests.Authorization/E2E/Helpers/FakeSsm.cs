using Amazon.SimpleSystemsManagement;
using Hackney.Core.Tests.Authorization.Helpers;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

// The E2E tests do not call the '.Verify(...)' on the mock, therefore they
// only need the object that mimicks the behaviour of mocked class.
internal static class FakeSsm
{
    public static IAmazonSimpleSystemsManagement WithPayload(string json) =>
        SsmMock.WithPayload(json).Object;
}
