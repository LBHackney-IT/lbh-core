using System;
using FluentAssertions;
using Hackney.Core.ElasticSearch;
using Xunit;

namespace Hackney.Core.Tests.ElasticSearch
{
    public class WildCardAppenderAndPrependerTests
    {
        private readonly WildCardAppenderAndPrepender _sut;

        public WildCardAppenderAndPrependerTests()
        {
            _sut = new WildCardAppenderAndPrepender();
        }

        [Theory]
        [InlineData("a")]
        [InlineData("a b")]
        [InlineData("a be cee")]
        public void GivenAPhraseWhenProcessedShouldAppendWildCardBeforeAndAfterEveryWord(string phrase)
        {
            // given + when
            var result = _sut.Process(phrase);

            // then
            var phraseArray = phrase.Split(" ");
            result.Count.Should().Be(result.Count);

            foreach (string word in phraseArray)
            {
                result.Contains("*" + word + "*").Should().BeTrue();
            }
        }
    }
}
