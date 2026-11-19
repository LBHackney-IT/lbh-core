using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Hackney.Core.Authorization.Internal
{
    internal class PolicyGroupMap
    {
        public FrozenDictionary<string, FrozenSet<string>> Policies { get; private set; }

        public PolicyGroupMap(Dictionary<string, HashSet<string>> policies)
        {
            if (policies is null)
                throw new ArgumentNullException(nameof(policies));

            Policies = policies.ToFrozenDictionary(
                static pair => pair.Key,
                static pair => pair.Value.ToFrozenSet(pair.Value.Comparer),
                policies.Comparer);
        }

        public void Replace(PolicyGroupMap loadedMap)
        {
            if (loadedMap is null)
                throw new ArgumentNullException(nameof(loadedMap));

            Policies = loadedMap.Policies;
        }
    }
}
