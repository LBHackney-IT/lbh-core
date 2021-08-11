using System.Collections.Generic;

namespace Hackney.Core.DynamoDb
{
    /// <summary>
    /// Class encapsulting the results of paged query against DynamoDb 
    /// </summary>
    /// <typeparam name="T">The database model class used.</typeparam>
    public class PagedResult<T> where T : class
    {
        /// <summary>
        /// The list of query results
        /// </summary>
        public List<T> Results { get; set; } = new List<T>();

        /// <summary>
        /// The pagination details
        /// </summary>
        public PaginationDetails PaginationDetails { get; set; } = new PaginationDetails();

        public PagedResult() { }
        public PagedResult(IEnumerable<T> results)
        {
            if (null != results) Results.AddRange(results);
        }
        public PagedResult(IEnumerable<T> results, PaginationDetails paginationDetails)
        {
            if (null != results) Results.AddRange(results);
            PaginationDetails = paginationDetails;
        }
    }
}
