using Classy.Interfaces.Search;

namespace classy.Extentions
{
    public static class IndexerExtensions
    {
        public static void Index<T>(this IIndexer<T> indexer, T entity, string appId)
        {
            indexer.Index(new [] { entity }, appId);
        }
    }
}