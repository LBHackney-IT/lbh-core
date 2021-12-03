using System.Collections.Generic;

namespace Hackney.Core.Testing.PactBroker
{
    /// <summary>
    /// Class describing a provider state passed by the broker to the provider-states route
    /// </summary>
    public class ProviderState
    {
        /// <summary>
        /// The State description.
        /// This is the exact text of the "Given" section for an interaction defined in a pact.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Additional parameters supplied. (Not used at present)
        /// </summary>
        public IDictionary<string, string> Params { get; set; }
    }
}
