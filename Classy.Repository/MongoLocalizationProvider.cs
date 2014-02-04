using Classy.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public class MongoLocalizationProvider : ILocalizationRepository
    {
        private MongoCollection<LocalizationResource> ResourcesCollection;
        private MongoCollection<LocalizationListResource> ListResourcesCollection;

        public MongoLocalizationProvider(MongoDatabase db)
        {
            ResourcesCollection = db.GetCollection<LocalizationResource>("resources");
            ListResourcesCollection = db.GetCollection<LocalizationListResource>("listresources");
        }
    
        public LocalizationResource GetResourceByKey(string appId, string key)
        {
            var query = Query<LocalizationResource>.Where(x => x.AppId == appId && x.Key == key);
            var resource = ResourcesCollection.FindOne(query);
            return resource;
        }

        public string SetResource(LocalizationResource resource)
        {
            ResourcesCollection.Save(resource);
            return resource.Id;
        }

        public LocalizationListResource GetListResourceByKey(string appId, string key)
        {
            var query = Query<LocalizationListResource>.Where(x => x.AppId == appId && x.Key == key);
            var resource = ListResourcesCollection.FindOne(query);
            return resource;
        }

        public string SetListResource(LocalizationListResource listResource)
        {
            ListResourcesCollection.Save(listResource);
            return listResource.Id;
        }

        public IList<string> GetResourceKeysForApp(string appId)
        {
            var query = Query<LocalizationResource>.Where(x => x.AppId == appId);
            var resourceKeys = ResourcesCollection.Find(query).Select(x => x.Key).ToList();
            return resourceKeys;
        }
    }
}
