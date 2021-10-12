using System;
using System.Collections.Generic;
using Hackney.Core.ElasticSearch.Interfaces;
using Nest;

namespace Hackney.Core.ElasticSearch
{
    public class QueryBuilder<T> : IQueryBuilder<T> where T : class
    {
        private readonly IWildCardAppenderAndPrepender _wildCardAppenderAndPrepender;
        private Func<QueryContainerDescriptor<T>, QueryContainer> _wildstarQuery;
        private Func<QueryContainerDescriptor<T>, QueryContainer> _exactQuery;
        private List<Func<QueryContainerDescriptor<T>, QueryContainer>> _filterQueries;


        public QueryBuilder(IWildCardAppenderAndPrepender wildCardAppenderAndPrepender, IExactSearchQuerystringProcessor exactQueryStringProcessor)
        {
            _wildCardAppenderAndPrepender = wildCardAppenderAndPrepender;
        }

        public IQueryBuilder<T> WithWildstarQuery(string searchText, List<string> fields)
        {
            var listOfWildCardedWords = _wildCardAppenderAndPrepender.Process(searchText);
            var queryString = $"({string.Join(" AND ", listOfWildCardedWords)}) " +
                              string.Join(' ', listOfWildCardedWords);

            _wildstarQuery = CreateQuery(queryString, fields);

            return this;
        }

        public IQueryBuilder<T> WithFilterQuery(string commaSeparatedFilters, List<string> fields)
        {
            if (commaSeparatedFilters != null)
            {
                _filterQueries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
                foreach (var filterWord in commaSeparatedFilters.Split(","))
                {
                    _filterQueries.Add(CreateQuery(filterWord, fields));
                }
            }

            return this;
        }

        public IQueryBuilder<T> WithExactQuery(string searchText, List<string> fields,
            IExactSearchQuerystringProcessor processor = null)
        {
            if (processor != null)
                searchText = processor.Process(searchText);

            _exactQuery = CreateQuery(searchText, fields, 20);

            return this;
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> CreateQuery(string queryString,
            List<string> fields, double? boostValue = null)
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> query =
                (containerDescriptor) => containerDescriptor.QueryString(q =>
                {
                    var queryDescriptor = q.Query(queryString)
                        .Type(TextQueryType.MostFields)
                        .Fields(f =>
                        {
                            foreach (var field in fields)
                            {
                                f = f.Field(field, boostValue);
                            }

                            return f;
                        });

                    return queryDescriptor;
                });

            return query;
        }

        public QueryContainer Build(QueryContainerDescriptor<T> containerDescriptor)
        {
            var queryContainer = containerDescriptor.Bool(x => x.Should(_wildstarQuery, _exactQuery));

            if (_filterQueries != null)
            {
                var listOfFunctions = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
                listOfFunctions.AddRange(_filterQueries);

                queryContainer = containerDescriptor.Bool(x =>
                    x.Must(containerDescriptor.Bool(x => x.Should(listOfFunctions)),
                    queryContainer));
            }

            return queryContainer;
        }
    }
}