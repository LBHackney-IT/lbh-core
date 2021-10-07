using System.Collections.Generic;

namespace Hackney.Core.Elastic.Interfaces
{
    public interface IWildCardAppenderAndPrepender
    {
        List<string> Process(string phrase);
    }
}