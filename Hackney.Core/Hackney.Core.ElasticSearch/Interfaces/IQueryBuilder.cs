using System.Collections.Generic;
using Nest;

namespace Hackney.Core.ElasticSearch.Interfaces
{
    public interface IQueryBuilder<T> where T : class
    {
        IQueryBuilder<T> CreateWildstarSearchQuery(string searchText);

        IQueryBuilder<T> CreateFilterQuery(string commaSeparatedFilters);

        IQueryBuilder<T> SpecifyFieldsToBeSearched(List<string> fields);

        IQueryBuilder<T> SpecifyFieldsToBeFiltered(List<string> fields);

        QueryContainer FilterAndRespectSearchScore(QueryContainerDescriptor<T> descriptor);

        QueryContainer Search(QueryContainerDescriptor<T> containerDescriptor);
    }
}
