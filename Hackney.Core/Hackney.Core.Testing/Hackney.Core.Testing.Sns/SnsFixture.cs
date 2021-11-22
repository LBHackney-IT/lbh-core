using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using System;
using System.Collections.Generic;

namespace Hackney.Core.Testing.Sns
{
    /// <summary>
    /// Sns fixture class to be used to set up a local Sns instance for use in tests where a 
    /// "real" instance is required.
    /// </summary>
    public class SnsFixture : ISnsFixture
    {
        /// <summary>
        /// An IAmazonSimpleNotificationService reference
        /// </summary>
        public IAmazonSimpleNotificationService SimpleNotificationService { get; private set; }

        /// <summary>
        /// An IAmazonSQS reference
        /// </summary>
        public IAmazonSQS AmazonSQS { get; private set; }

        private readonly Dictionary<string, ISnsEventVerifier> _snsVerifers;

        public SnsFixture(IAmazonSimpleNotificationService simpleNotificationService)
        {
            SimpleNotificationService = simpleNotificationService;
            var localstackUrl = Environment.GetEnvironmentVariable("Localstack_SnsServiceUrl");
            AmazonSQS = new AmazonSQSClient(new AmazonSQSConfig() { ServiceURL = localstackUrl });

            _snsVerifers = new Dictionary<string, ISnsEventVerifier>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var verifier in _snsVerifers)
                    verifier.Value.Dispose();

                _disposed = true;
            }
        }

        /// <summary>
        /// Retrieves the SnsEventVerifier appropriate to the specified event type.
        /// </summary>
        /// <typeparam name="T">The type used for the event payload</typeparam>
        /// <returns><see cref="ISnsEventVerifier"/> reference or null</returns>
        public ISnsEventVerifier GetSnsEventVerifier<T>() where T : class
        {
            var name = typeof(T).Name;
            return _snsVerifers.ContainsKey(name) ? _snsVerifers[name] : null;
        }

        /// <summary>
        /// Creates the required Sns topic in the configured Sns instance. 
        /// Also creates an <see cref="SnsEventVerifier"/> for the topic.
        /// </summary>
        /// <typeparam name="T">The type used for the event payload</typeparam>
        /// <param name="topicName">The topic name required</param>
        /// <param name="topicArnEnvVarName">The name of the environment variable against which the created topic arn will be set.</param>
        /// <param name="snsAttrs">(Optional) List of additional attributes to use in the topic creation.</param>
        /// <returns>Task</returns>
        /// <exception cref="System.ArgumentNullException">If the topicName or topicArnEnvVarName are not provided</exception>
        public void CreateSnsTopic<T>(string topicName, string topicArnEnvVarName, Dictionary<string, string> snsAttrs = null) where T : class
        {
            if (string.IsNullOrEmpty(topicName)) throw new ArgumentNullException(nameof(topicName));
            if (string.IsNullOrEmpty(topicArnEnvVarName)) throw new ArgumentNullException(nameof(topicArnEnvVarName));

            snsAttrs = snsAttrs ?? new Dictionary<string, string>();
            snsAttrs.Add("fifo_topic", "true");
            snsAttrs.Add("content_based_deduplication", "true");

            var response = SimpleNotificationService.CreateTopicAsync(new CreateTopicRequest
            {
                Name = topicName,
                Attributes = snsAttrs
            }).Result;

            Environment.SetEnvironmentVariable(topicArnEnvVarName, response.TopicArn);

            _snsVerifers[typeof(T).Name] = (new SnsEventVerifier(AmazonSQS, SimpleNotificationService, response.TopicArn));
        }
    }
}
