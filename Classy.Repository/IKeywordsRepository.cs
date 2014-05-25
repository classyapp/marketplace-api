namespace Classy.Repository
{
    public interface IKeywordsRepository
    {
        void IncrementCount(string key, string lang, int amount = 1);
    }
}