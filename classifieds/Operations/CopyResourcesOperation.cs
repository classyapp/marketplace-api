using classy.Extentions;
using Classy.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using MongoDB.Driver.Linq;

namespace classy.Operations
{
    internal class CopyResourcesOperation
    {
        private static MongoDatabase GetMongoDB(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            var server = client.GetServer();
            var db = server.GetDatabase(databaseName);
            return db;
        }
        internal static void CopyResources()
        {
            var copyMongoUri = ConfigurationManager.AppSettings["RESOURCE_COPY_MONGO_URI"];
            if (string.IsNullOrEmpty(copyMongoUri))
            {
                return;
            }

            var mongoDbFrom = GetMongoDB(copyMongoUri);
            var mongoUri = FunqExtensions.GetConnectionString("MONGO");
            var mongoDbTo = GetMongoDB(mongoUri);
        }
        private static void CopySingleResources(MongoDatabase mongoDbFrom, MongoDatabase mongoDbTo)
        {
            var fromResources = mongoDbFrom.GetCollection<LocalizationResource>("resources").FindAll();
            var toCollection = mongoDbTo.GetCollection<LocalizationResource>("resources");
            foreach (var fromResource in fromResources)
            {
                var toResource = toCollection.AsQueryable().SingleOrDefault(x => x.Key == fromResource.Key && x.AppId == fromResource.AppId);
                if (toResource == null)
                {
                    // add resource
                    toCollection.Insert(new LocalizationResource()
                    {
                        AppId = fromResource.AppId,
                        ContextScreenshotUrl = fromResource.ContextScreenshotUrl,
                        ContextUrl = fromResource.ContextUrl,
                        Description = fromResource.Description,
                        Key = fromResource.Key,
                        Values = fromResource.Values
                    });
                }
                else
                {
                    // resource exists, check every value
                    foreach (var val in fromResource.Values)
                    {
                        if (!toResource.Values.ContainsKey(val.Key))
                        {
                            toResource.Values.Add(val);
                        }
                    }
                    toCollection.Save(toResource);
                }
            }
        }
    }
}