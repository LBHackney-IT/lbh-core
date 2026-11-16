using System.Threading;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Moq;

namespace Hackney.Core.Tests.Authorization.Helpers;

// Returns the whole mock, not just the object because the non-E2E tests need to run assertions
// of what happened against the mock.Object via the '.Verify(...)' extension method. 
internal static class SsmMock
{
    public static Mock<IAmazonSimpleSystemsManagement> WithPayload(string json)
    {
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();

        ssm.Setup(client => client.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse
            {
                Parameter = new Parameter { Value = json }
            });

        return ssm;
    }
}
