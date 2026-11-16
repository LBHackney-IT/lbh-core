using System;
using System.Collections.Generic;
using System.Text.Json;
using Hackney.Core.Authorization.Exceptions;

namespace Hackney.Core.Authorization.Internal
{
    internal static class PolicyGroupMapMapper
    {
        public static PolicyGroupMap Map(string json)
        {
            Dictionary<string, string[]>? raw;
            try
            {
                raw = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
            }
            catch (JsonException ex)
            {
                throw new InvalidGroupDocumentException("The group document is not a policy map.", ex);
            }

            if (raw == null)
                throw new InvalidGroupDocumentException("The group document could not be deserialized.");

            var policies = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, groups) in raw)
            {
                if (groups is null)
                    throw new InvalidGroupDocumentException($"Policy '{key}' has no group list.");

                policies[key] = new HashSet<string>(groups, StringComparer.Ordinal);
            }

            return new PolicyGroupMap(policies);
        }
    }
}
