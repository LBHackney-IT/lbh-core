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
        IAmazonSimpleNotificationService SimpleNotificationService { get; }
        IAmazonSQS AmazonSQS { get; }

        ISnsEventVerifier GetSnsEventVerifier<T>() where T : class;
        void CreateSnsTopic<T>(string topicName, string topicArnEnvVarName, Dictionary<string, string> snsAttrs = null) where T : class;
    }
}
