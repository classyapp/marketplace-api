namespace classy.Cache
{
    public interface ICache<T>
    {
        void Add(string key, T value);
        void Add(string key, T value, int timeToExpire);
        T Get(string key);
    }
}