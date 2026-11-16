using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Hackney.Core.Authorization.Internal
{
    internal static class PolicyGroupMapMapper
    {
        public static PolicyGroupMap Map(string json)
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
            if (raw == null)
                throw new InvalidOperationException("SSM Parameter payload could not be deserialized.");

            var policies = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, groups) in raw)
                policies[key] = new HashSet<string>(groups, StringComparer.Ordinal);

            return new PolicyGroupMap(policies);
        }
    }
}
