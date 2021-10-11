using Hackney.Core.ElasticSearch.Interfaces;
using System.Collections.Generic;

namespace Hackney.Core.ElasticSearch
{
    /// <summary>
    /// Class to use to convert a search phrase into a list of wildcard words for use with ElasticSearch
    /// </summary>
    public class WildCardAppenderAndPrepender : IWildCardAppenderAndPrepender
    {
        /// <summary>
        /// Splits the input search string by [space] and converts each search term into a wildcard
        /// word to use with ElasticSearch.
        /// </summary>
        /// <param name="phrase">Search terms [space] seperated</param>
        /// <returns>List of wildcard words or an empty list if the input is null or empty</returns>
        public List<string> Process(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                return new List<string>();
            }

            var listOfWildcardWords = new List<string>();
            foreach (var word in phrase.Split(' '))
            {
                listOfWildcardWords.Add($"*{word}*");
            }

            return listOfWildcardWords;
        }
    }
}