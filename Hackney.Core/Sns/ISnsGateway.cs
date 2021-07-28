using System.Threading.Tasks;

namespace Hackney.Core.Sns
{
    public interface ISnsGateway
    {
        Task Publish<T>(T snsMessage, string topicArn, string messageGroupId = "fake") where T : class;
    }
}
