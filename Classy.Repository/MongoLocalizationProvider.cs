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

        public MongoLocalizationProvider(MongoDatabase db)
        {
            ResourcesCollection = db.GetCollection<LocalizationResource>("resources");
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
    }
}
