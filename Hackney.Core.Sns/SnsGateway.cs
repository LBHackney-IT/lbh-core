using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hackney.Core.Sns
{
    /// <summary>
    /// Class implementing the ability to raise an event to AWS SNS
    /// </summary>
    public class SnsGateway : ISnsGateway
    {
        private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService;
        private readonly JsonSerializerOptions _jsonOptions;

        public SnsGateway(IAmazonSimpleNotificationService amazonSimpleNotificationService)
        {
            _amazonSimpleNotificationService = amazonSimpleNotificationService;
            _jsonOptions = CreateJsonOptions();
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        /// <summary>
        /// Publishes an event message to the specified SNS topic
        /// </summary>
        /// <typeparam name="T">The type of message object</typeparam>
        /// <param name="snsMessage">The message object</param>
        /// <param name="topicArn">The topic arn to use</param>
        /// <param name="messageGroupId">Optional message group id</param>
        /// <returns>Task</returns>
        /// <exception cref="System.ArgumentNullException">If snsMessage is null or the topicArn is null or empty.</exception>
        public async Task Publish<T>(T snsMessage, string topicArn, string messageGroupId = "fake") where T : class
        {
            if (snsMessage is null) throw new ArgumentNullException(nameof(snsMessage));
            if (string.IsNullOrEmpty(topicArn)) throw new ArgumentNullException(nameof(topicArn));

            string message = JsonSerializer.Serialize(snsMessage, _jsonOptions);
            var request = new PublishRequest
            {
                Message = message,
                TopicArn = topicArn,
                MessageGroupId = messageGroupId
            };

            await _amazonSimpleNotificationService.PublishAsync(request).ConfigureAwait(false);
        }
    }
}