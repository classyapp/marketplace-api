using classy.Cache;
using Classy.Models;
using Classy.Repository.Infrastructure;
using MongoDB.Driver;

namespace classy.Manager
{
    public class DefaultAppManager : IAppManager
    {
        private readonly MongoCollection<App> _appCollection;
        private readonly ICache<App> _appCache;

        public DefaultAppManager(MongoDatabaseProvider db, ICache<App> appCache)
        {
            _appCollection = db.GetCollection<App>();
            _appCache = appCache;
        }

        public App GetAppById(string appId)
        {
            var cache = _appCache.Get(appId);
            if (cache != null)
                return cache;

            var app = _appCollection.FindOne(MongoDB.Driver.Builders.Query<App>.EQ(x => x.AppId, appId));
            _appCache.Add(appId, app);

            return app;
        }
    }
}