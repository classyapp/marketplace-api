using Nest;

namespace Classy.Interfaces.Search
{
    public interface ISearchClientFactory
    {
        ElasticClient GetClient(string indexName, string appId);
    }
}