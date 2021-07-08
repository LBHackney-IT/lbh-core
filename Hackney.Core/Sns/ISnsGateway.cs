using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackney.Core.Sns
{
    public interface ISnsGateway
    {
        Task Publish<T>(T SnsMessage, string topicArn, string messageGroupId = "fake");
    }
}
