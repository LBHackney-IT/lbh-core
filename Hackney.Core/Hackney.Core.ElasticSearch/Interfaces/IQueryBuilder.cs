using System.Collections.Generic;
using Nest;

namespace Hackney.Core.ElasticSearch.Interfaces
{
    public interface IQueryBuilder<T> where T : class
    {
        public IQueryBuilder<T> WithWildstarQuery(string searchText, List<string> fields);

        public IQueryBuilder<T> WithFilterQuery(string commaSeparatedFilters, List<string> fields);

        public IQueryBuilder<T> WithExactQuery(string searchText, List<string> fields, IExactSearchQuerystringProcessor processor);

        public QueryContainer Build(QueryContainerDescriptor<T> containerDescriptor);
    }
}
