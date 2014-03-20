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

        public void Delete(string appId, string collectionId)
        {
            try
            {
                var query = Query<Collection>.Where(x => x.AppId == appId && x.Id == collectionId);
                CollectionsCollection.Remove(query);
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public IList<Collection> GetByProfileId(string appId, string profileId, string collectionType)
        {
            try
            {
                var getByProfileId = Query<Collection>.Where(x => x.AppId == appId && x.ProfileId == profileId && x.Type == collectionType);
                var collections = CollectionsCollection.Find(getByProfileId);
                return collections.ToList();
            }
            catch (MongoException)
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

        public void RemoveListingById(string appId, string listingId)
        {
            try
            {
                var getByProfileId = Query<Collection>.Where(x => x.AppId == appId && x.IncludedListings.Any(y => y.Id == listingId));
                CollectionsCollection.Update(getByProfileId, MongoDB.Driver.Builders.Update.Pull("IncludedListings", Query.EQ("ListingId", listingId)));
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public void SubmitForEditorialApproval(string appId, string collectionId)
        {
            try
            {
                CollectionsCollection.Update(Query<Collection>.Where(x => x.AppId == appId && x.Id == collectionId), Update<Collection>.Set(x => x.SumittedForEditorialApproval, true));
            }
            catch (MongoException)
            {
                throw;
            }
        }


        public void EditorialApproveCollection(string appId, string collectionId, string editorProfileId, string category)
        {
            try
            {
                CollectionsCollection.Update(
                    Query<Collection>.Where(x => x.AppId == appId && x.Id == collectionId), 
                    Update<Collection>
                        .Set(x => x.EditorialApprovalBy, editorProfileId)
                        .Set(x => x.EditorialApprovalDate, DateTime.UtcNow)
                        .Set(x => x.Category, category)
                );
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public IList<Collection> GetApprovedCollections(string appId, string[] categories, int maxCollections)
        {
            try
            {
                var query = Query<Collection>.Where(x => x.AppId == appId && x.EditorialApprovalDate != null);
                if (categories != null) query = Query.And(query, Query<Collection>.In(x => x.Category, categories));

                var collections = CollectionsCollection.Find(query)
                    .SetSortOrder(SortBy<Collection>.Descending(x => x.EditorialApprovalDate))
                    .SetLimit(maxCollections);
                return collections.ToList();
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public void IncreaseCounter(string collectionId, string appId, CollectionCounters counters, int value)
        {
            try
            {
                var query = Query.And(
                    Query<Collection>.EQ(x => x.Id, collectionId),
                    Query<Collection>.EQ(x => x.AppId, appId)
                    );
                var update = new UpdateBuilder<Collection>();
                if (counters.HasFlag(CollectionCounters.Comments)) update.Inc(x => x.CommentCount, value);
                CollectionsCollection.Update(query, update);
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void AddHashtags(string collectionId, string appId, string[] hashtags)
        {
            try
            {
                CollectionsCollection.Update(Query.And(
                    Query<Collection>.EQ(x => x.Id, collectionId),
                    Query<Collection>.EQ(x => x.AppId, appId)), Update<Collection>.AddToSetEach<string>(x => x.Hashtags, hashtags));
            }
            catch (MongoException mex)
            {
                throw;
            }
        }
    }
}