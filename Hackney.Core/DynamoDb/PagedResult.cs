using System.Collections.Generic;

namespace Hackney.Core.DynamoDb
{
    public class PagedResult<T> where T : class
    {
        public List<T> Results { get; set; } = new List<T>();

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
