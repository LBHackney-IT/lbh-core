using System;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.Sns
{
    /// <summary>
    /// Sns event verifier interface to be used to verify that an expected event is raised against the specified topic.
    /// </summary>
    public interface ISnsEventVerifier : IDisposable
    {
        /// <summary>
        /// The last exception encountered whilst processing <see cref="ISnsEventVerifier.VerifySnsEventRaised{T}(Action{T})"/>
        /// </summary>
        Exception LastException { get; }

        /// <summary>
        /// Verifies that the an expected event has been raised to the topic associated with this verifier.
        /// </summary>
        /// <typeparam name="T">The event class used to create the event.</typeparam>
        /// <param name="verifyFunction">A function that will receive a copy of each event raised.
        /// This function should attempt to verify that the contents of the message match what is expected.
        /// Throw an exception should then contents not match.
        /// </param>
        /// <returns>true if a message in the temporary queue satisfies the verification function.
        /// false if no message in the temporary queue satisfies the verification function.</returns>
        /// <exception cref="System.Exception">If no SQS messages are found.</exception>
        Task<bool> VerifySnsEventRaised<T>(Action<T> verifyFunction) where T : class;

        /// <summary>
        /// Purges all message from the temporary queue created for this verifier.
        /// </summary>
        /// <returns>Task</returns>
        Task PurgeQueueMessages();
    }
}
