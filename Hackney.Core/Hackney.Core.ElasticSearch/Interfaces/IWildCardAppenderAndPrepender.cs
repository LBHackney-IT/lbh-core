using System.Collections.Generic;

namespace Hackney.Core.ElasticSearch.Interfaces
{
    public interface IWildCardAppenderAndPrepender
    {
        List<string> Process(string phrase);
    }
}