using Hackney.Core.ElasticSearch.Interfaces;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.ElasticSearch
{
    /// <summary>
    /// TODO...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryBuilder<T> : IQueryBuilder<T> where T : class
    {
        private readonly IWildCardAppenderAndPrepender _wildCardAppenderAndPrepender;
        private readonly List<Func<QueryContainerDescriptor<T>, QueryContainer>> _queries;
        private string _searchQuery;
        private string _filterQuery;

        public QueryBuilder(IWildCardAppenderAndPrepender wildCardAppenderAndPrepender)
        {
            _wildCardAppenderAndPrepender = wildCardAppenderAndPrepender;
            _queries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
        }

        /// <summary>
        /// TODO...
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public IQueryBuilder<T> CreateWildstarSearchQuery(string searchText)
        {
            var listOfWildCardedWords = _wildCardAppenderAndPrepender.Process(searchText);
            _searchQuery = $"({string.Join(" AND ", listOfWildCardedWords)}) " +
                           string.Join(' ', listOfWildCardedWords);

            return this;
        }

        /// <summary>
        /// TODO...
        /// </summary>
        /// <param name="commaSeparatedFilters"></param>
        /// <returns></returns>
        public IQueryBuilder<T> CreateFilterQuery(string commaSeparatedFilters)
        {
            _filterQuery = string.Join(' ', commaSeparatedFilters.Split(","));

            return this;
        }

        /// <summary>
        /// TODO...
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IQueryBuilder<T> SpecifyFieldsToBeSearched(List<string> fields)
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> query =
                (containerDescriptor) => containerDescriptor.QueryString(q =>
                {
                    var queryDescriptor = q.Query(_searchQuery)
                        .Type(TextQueryType.MostFields)
                        .Fields(f =>
                        {
                            foreach (var field in fields)
                            {
                                f = f.Field(field);
                            }

                            return f;
                        });

                    return queryDescriptor;
                });

            _queries.Add(query);

            return this;
        }

        /// <summary>
        /// TODO...
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IQueryBuilder<T> SpecifyFieldsToBeFiltered(List<string> fields)
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> query =
                (containerDescriptor) => containerDescriptor.QueryString(q =>
                {
                    var queryDescriptor = q.Query(_filterQuery)
                        .Type(TextQueryType.MostFields)
                        .Fields(f =>
                        {
                            foreach (var field in fields)
                            {
                                f = f.Field(field);
                            }

                            return f;
                        });

                    return queryDescriptor;
                });

            _queries.Add(query);

            return this;
        }

        /// <summary>
        /// TODO...
        /// </summary>
        /// <param name="containerDescriptor"></param>
        /// <returns></returns>
        public QueryContainer FilterAndRespectSearchScore(QueryContainerDescriptor<T> containerDescriptor)
        {
            return containerDescriptor.Bool(builder => builder.Must(_queries));
        }

        /// <summary>
        /// TODO...
        /// </summary>
        /// <param name="containerDescriptor"></param>
        /// <returns></returns>
        public QueryContainer Search(QueryContainerDescriptor<T> containerDescriptor)
        {
            return _queries.First().Invoke(containerDescriptor);
        }
    }
}