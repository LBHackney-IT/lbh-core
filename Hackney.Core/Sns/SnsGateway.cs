using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;

namespace Hackney.Core.Sns
{
    public class SnsGateway : ISnsGateway
    {
        private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService;

        public SnsGateway(IAmazonSimpleNotificationService amazonSimpleNotificationService)
        {
            _amazonSimpleNotificationService = amazonSimpleNotificationService;
        }

        public async Task Publish<T>(T contactDetailsSns, string topicArn, string messageGroupId = "fake")
        {
            string message = JsonConvert.SerializeObject(contactDetailsSns);

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