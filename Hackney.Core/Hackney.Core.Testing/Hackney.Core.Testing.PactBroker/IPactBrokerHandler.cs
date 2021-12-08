using System;
using System.Collections.Generic;

namespace Hackney.Core.Testing.PactBroker
{
    /// <summary>
    /// Interface describing what a pact broker handler must implement in order to be used
    /// by the custom pact broker middleware
    /// </summary>
    public interface IPactBrokerHandler : IDisposable
    {
        /// <summary>
        /// Dictionary of supported pact states
        /// </summary>
        IDictionary<string, PactStateHandler> ProviderStates { get; }
    }

    /// <summary>
    /// Delegate representing the state details sent by the pact broker to the provider-states 
    /// route so that the API might prepare for the subsequent API call made by the broker.
    /// </summary>
    /// <param name="state">The state description - this is the "Given" text from a pact interaction</param>
    /// <param name="args">Any additional arguments sent (not currently in use)</param>
    public delegate void PactStateHandler(string state, IDictionary<string, string> args);
}
