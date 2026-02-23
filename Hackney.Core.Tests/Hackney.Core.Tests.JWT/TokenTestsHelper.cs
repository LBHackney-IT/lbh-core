using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.Tests.JWT
{
    internal static class TokenTestsHelper
    {
        public static char CognitoTokenGoogleGroupsSeparator => ';';

        private static IEnumerable<string> GenerateGoogleGroups(int count)
        {
            if (count < 1) return Enumerable.Empty<string>();

            return Enumerable.Range(1, count).Select(i => $"Google-Group-{i}").ToList();
        }

        private static string GenerateCognitoTokenGroupsString(int count)
        {
            var googleGroups = GenerateGoogleGroups(count);

            return string.Join(CognitoTokenGoogleGroupsSeparator, googleGroups);
        }
    }
}
