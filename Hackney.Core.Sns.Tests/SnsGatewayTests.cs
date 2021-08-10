using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Moq;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Sns.Tests
{
    public class SnsGatewayTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IAmazonSimpleNotificationService> _mockAwsSns;
        private readonly SnsGateway _sut;
        private readonly SnsTestMessage _message;
        private readonly JsonSerializerOptions _jsonOptions;

        private readonly string _topic = "some-topic";
        private readonly string _group = "some-group";

        public SnsGatewayTests()
        {
            _mockAwsSns = new Mock<IAmazonSimpleNotificationService>();
            _sut = new SnsGateway(_mockAwsSns.Object);
            _message = _fixture.Create<SnsTestMessage>();
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

        private static bool VerifyPublishRequest(PublishRequest actual, string expectedMsgJson, string expectedTopic, string expectedGroup)
        {
            actual.Message.Should().Be(expectedMsgJson);
            actual.TopicArn.Should().Be(expectedTopic);
            actual.MessageGroupId.Should().Be(expectedGroup);
            return true;
        }

        [Fact]
        public void PublishTestNullSnsObjectThrows()
        {
            Func<Task> func = async () => await _sut.Publish<SnsTestMessage>(null, _topic, _group).ConfigureAwait(false);
            func.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void PublishTestNullTopicArnThrows(string topic)
        {
            Func<Task> func = async () => await _sut.Publish<SnsTestMessage>(_message, topic, _group).ConfigureAwait(false);
            func.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a_group")]
        public async Task PublishTestPublishesMessageToTopic(string group)
        {
            var msgJson = JsonSerializer.Serialize(_message, _jsonOptions);
            await _sut.Publish<SnsTestMessage>(_message, _topic, group).ConfigureAwait(false);

            _mockAwsSns.Verify(x => x.PublishAsync(It.Is<PublishRequest>(y => VerifyPublishRequest(y, msgJson, _topic, group)), default), Times.Once);
        }
    }

    public class SnsTestMessage
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
