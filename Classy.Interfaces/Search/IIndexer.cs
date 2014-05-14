namespace Classy.Interfaces.Search
{
    public interface IIndexer<in T>
    {
        void Index(T[] entities);
    }
}