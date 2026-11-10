using System;
using System.Collections.Generic;

namespace Hackney.Core.Authorization.Internal
{
    internal class PolicyGroupMap
    {
        public Dictionary<string, HashSet<string>> Policies { get; }

        public PolicyGroupMap(Dictionary<string, HashSet<string>> policies)
        {
            Policies = policies ?? throw new ArgumentNullException(nameof(policies));
        }
    }
}
