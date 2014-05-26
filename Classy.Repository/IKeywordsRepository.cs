namespace Classy.Repository
{
    public interface IKeywordsRepository
    {
        void IncrementCount(string key, string lang, string appId, int amount = 1, bool upsert = false);
    }
}