using Classy.Models.Keywords;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Classy.Repository
{
    public class KeywordsRepository : IKeywordsRepository
    {
        private readonly MongoCollection<Keyword> KeywordsCollection;

        public KeywordsRepository(MongoDatabase db)
        {
            KeywordsCollection = db.GetCollection<Keyword>("classifieds");
        }

        public void IncrementCount(string key, string lang, int amount = 1)
        {
            KeywordsCollection.Update(
                Query.And(
                    Query<Keyword>.EQ(x => x.Name, key),
                    Query<Keyword>.EQ(x => x.Language, lang)
                ), new UpdateBuilder<Keyword>().Inc(x => x.Count, amount));
        }
    }
}