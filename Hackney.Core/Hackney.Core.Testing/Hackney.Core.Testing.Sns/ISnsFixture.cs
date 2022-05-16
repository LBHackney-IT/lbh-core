using Amazon.SimpleNotificationService;
using Amazon.SQS;
using System;
using System.Collections.Generic;

namespace Hackney.Core.Testing.Sns
{
    /// <summary>
    /// Sns fixture interface to be used to set up a local Sns instance for use in tests where a 
    /// "real" instance is required.
    /// </summary>
    public interface ISnsFixture : IDisposable
    {
        /// <summary>
        /// An IAmazonSimpleNotificationService reference
        /// </summary>
        IAmazonSimpleNotificationService SimpleNotificationService { get; }

        /// <summary>
        /// An IAmazonSQS reference
        /// </summary>
        IAmazonSQS AmazonSQS { get; }

        /// <summary>
        /// Retrieves the SnsEventVerifier appropriate to the specified event type.
        /// </summary>
        /// <typeparam name="T">The type used for the event payload</typeparam>
        /// <returns><see cref="ISnsEventVerifier"/> reference or null</returns>
        ISnsEventVerifier GetSnsEventVerifier<T>() where T : class;

        void PurgeAllQueueMessages();

        /// <summary>
        /// Creates the required Sns topic in the configured Sns instance. 
        /// Also creates an <see cref="SnsEventVerifier"/> for the topic.
        /// </summary>
        /// <typeparam name="T">The type used for the event payload</typeparam>
        /// <param name="topicName">The topic name required</param>
        /// <param name="topicArnEnvVarName">The name of the environment variable against which the created topic arn will be set.</param>
        /// <param name="snsAttrs">(Optional) List of additional attributes to use in the topic creation.</param>
        /// <returns>Task</returns>
        void CreateSnsTopic<T>(string topicName, string topicArnEnvVarName, Dictionary<string, string> snsAttrs = null) where T : class;
    }
}
