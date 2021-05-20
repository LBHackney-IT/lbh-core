using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Hackney.Core.DynamoDb
{
    public static class DynamoDBContextExtensions
    {
        // Note:
        // This method cannot be unit tested because IDynamoDBContext.GetTargetTable returns and object that cannot be mocked.
        // See here: https://github.com/aws/aws-sdk-net/issues/1310

        /// <summary>
        /// Executes a paged query against DyanmoDb for the specified entity
        /// </summary>
        /// <typeparam name="TEntity">The database model class</typeparam>
        /// <param name="dynamoDbContext">The IDynamoDBContext context</param>
        /// <param name="queryConfig">The query config used for the query</param>
        /// <returns>The paged results</returns>
        [ExcludeFromCodeCoverage]
        public static async Task<PagedResult<TEntity>> GetPagedQueryResultsAsync<TEntity>(
            this IDynamoDBContext dynamoDbContext, QueryOperationConfig queryConfig) where TEntity : class
        {
            var dbResults = new List<TEntity>();
            var table = dynamoDbContext.GetTargetTable<TEntity>();

            var search = table.Query(queryConfig);
            var resultsSet = await search.GetNextSetAsync().ConfigureAwait(false);
            var paginationToken = search.PaginationToken;
            if (resultsSet.Any())
            {
                dbResults.AddRange(dynamoDbContext.FromDocuments<TEntity>(resultsSet));

                // Look ahead for any more, but only if we have a token
                if (!string.IsNullOrEmpty(PaginationDetails.EncodeToken(paginationToken)))
                {
                    queryConfig.PaginationToken = paginationToken;
                    queryConfig.Limit = 1;
                    search = table.Query(queryConfig);
                    resultsSet = await search.GetNextSetAsync().ConfigureAwait(false);
                    if (!resultsSet.Any())
                        paginationToken = null;
                }
            }

            return new PagedResult<TEntity>(dbResults, new PaginationDetails(paginationToken));
        }
    }
}
