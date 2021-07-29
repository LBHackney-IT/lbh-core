using System.Threading.Tasks;

namespace Hackney.Core.Sns
{
    /// <summary>
    /// Interface defining the ability to raise an event to AWS SNS
    /// </summary>
    public interface ISnsGateway
    {
        /// <summary>
        /// Publishes an event message to the specified SNS topic
        /// </summary>
        /// <typeparam name="T">The type of message object</typeparam>
        /// <param name="snsMessage">The message object</param>
        /// <param name="topicArn">The topic arn to use</param>
        /// <param name="messageGroupId">Optional message group id</param>
        /// <returns>Task</returns>
        /// <exception cref="System.ArgumentNullException">If snsMessage is null or the topicArn is null or empty.</exception>
        Task Publish<T>(T snsMessage, string topicArn, string messageGroupId = "fake") where T : class;
    }
}
