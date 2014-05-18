using Classy.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace classy.Manager
{
    public class DefaultAppManager : IAppManager
    {
        private readonly MongoCollection<App> _appCollection;

        public DefaultAppManager(MongoDatabase db)
        {
            _appCollection = db.GetCollection<App>("apps");
        }

        public App GetAppById(string appId)
        {
            return _appCollection.FindOne(Query<App>.EQ(x => x.AppId, appId));
        }
    }
}