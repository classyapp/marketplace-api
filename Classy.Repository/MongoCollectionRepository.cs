using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver.Builders;
using Classy.Models;
using System.IO;

namespace Classy.Repository
{
    public class MongoCollectionRepository : ICollectionRepository
    {
        #region // fields and ctors

        private MongoCollection<Collection> CollectionsCollection;

        public MongoCollectionRepository(MongoDatabase db)
        {
            CollectionsCollection = db.GetCollection<Collection>("collections");
        }

        #endregion 
    
        public string Insert(Collection collection)
        {
            try
            {
                CollectionsCollection.Insert(collection);
                return collection.Id;
            }
            catch(MongoException)
            {
                throw;
            }
        }

        public void Update(Collection collection)
        {
            try
            {
                CollectionsCollection.Save(collection);
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public Collection GetById(string appId, string collectionId)
        {
            try
            {
                var getById = Query<Collection>.Where(x => x.AppId == appId && x.Id == collectionId);
                var collection = CollectionsCollection.FindOne(getById);
                return collection;
            }
            catch(MongoException)
            {
                throw;
            }
        }

        public IList<Collection> GetByProfileId(string appId, string profileId)
        {
            try
            {
                var getByProfileId = Query<Collection>.Where(x => x.AppId == appId && x.ProfileId == profileId);
                var collections = CollectionsCollection.Find(getByProfileId);
                return collections.ToList();
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public void RemoveListingById(string appId, string profileId, string listingId)
        {
            try
            {
                var getByProfileId = Query<Collection>.Where(x => x.AppId == appId && x.ProfileId == profileId && x.IncludedListings.Any(y => y.ListingId == listingId));
                CollectionsCollection.Update(getByProfileId, MongoDB.Driver.Builders.Update.Pull("IncludedListings", Query.EQ("ListingId", listingId)));
            }
            catch (MongoException)
            {
                throw;
            }
        }
    }
}