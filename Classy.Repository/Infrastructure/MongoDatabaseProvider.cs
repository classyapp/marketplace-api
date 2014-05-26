using System;
using Classy.Models;
using Classy.Models.Attributes;
using MongoDB.Driver;

namespace Classy.Repository.Infrastructure
{
    public class MongoDatabaseProvider
    {
        private readonly MongoDatabase _db;

        public MongoDatabaseProvider(MongoDatabase db)
        {
            _db = db;
        }

        public MongoCollection<T> GetCollection<T>() where T : BaseObject
        {
            var attributes = typeof (T).GetCustomAttributes(typeof (MongoCollectionAttribute), true);
            var collectionAttribute = attributes[0] as MongoCollectionAttribute;
            
            if (collectionAttribute == null)
                throw new Exception("Type does not have a MongoCollectionAttribute");

            return _db.GetCollection<T>(collectionAttribute.Name);
        }
    }
}