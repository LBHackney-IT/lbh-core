using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hackney.Core.Sns
{
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