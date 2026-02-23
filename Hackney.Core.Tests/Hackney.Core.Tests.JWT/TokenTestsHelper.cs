using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Hackney.Core.JWT;

namespace Hackney.Core.Tests.JWT
{
    internal static class TokenTestsHelper
    {
        public static char CognitoTokenGoogleGroupsSeparator => ';';
        private static Fixture _fixture = new Fixture();

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

        private static TokenPresentation GenerateTestTokenPresentationObj()
        {
            return _fixture.Build<TokenPresentation>()
                .With(x => x.CustomGroups, GenerateCognitoTokenGroupsString(count: 5))
                .Create();
        }

        private static Token GenerateTestTokenObj()
        {
            return _fixture.Build<Token>()
                .With(x => x.Groups, GenerateGoogleGroups(count: 5))
                .Create();
        }
    }
}
