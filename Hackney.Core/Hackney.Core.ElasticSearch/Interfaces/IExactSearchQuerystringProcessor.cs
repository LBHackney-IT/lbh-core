namespace Hackney.Core.ElasticSearch.Interfaces
{
    public interface IExactSearchQuerystringProcessor
    {
        string Process(string searchText);
    }
}