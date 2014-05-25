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
            KeywordsCollection = db.GetCollection<Keyword>("keywords");
        }

        // TODO: make this method thread safe!
        public void IncrementCount(string key, string lang, string appId, int amount = 1, bool upsert = false)
        {
            var result = KeywordsCollection.Update(
                Query.And(
                    Query<Keyword>.EQ(x => x.Name, key),
                    Query<Keyword>.EQ(x => x.Language, lang),
                    Query<Keyword>.EQ(x => x.AppId, appId)
                ), new UpdateBuilder<Keyword>().Inc(x => x.Count, amount));

            if (upsert && !result.UpdatedExisting)
                KeywordsCollection.Insert(new Keyword {
                    Name = key,
                    Language = lang,
                    Count = 1,
                    AppId = appId
                });
        }
    }
}