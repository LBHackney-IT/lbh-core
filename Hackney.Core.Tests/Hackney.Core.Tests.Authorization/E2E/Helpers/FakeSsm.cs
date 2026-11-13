using System.Threading;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Moq;

namespace Hackney.Core.Tests.Authorization.E2E.Helpers;

internal static class FakeSsm
{
    public static IAmazonSimpleSystemsManagement WithPayload(string json)
    {
        var ssm = new Mock<IAmazonSimpleSystemsManagement>();
        ssm.Setup(client => client.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse
            {
                Parameter = new Parameter { Value = json }
            });

        return ssm.Object;
    }
}
