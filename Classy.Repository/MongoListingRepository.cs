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
    public class MongoListingRepository : IListingRepository
    {
        static MongoClient Client = new MongoClient("mongodb://localhost");
        static MongoServer Server;
        static MongoDatabase Db;
        static MongoCollection<Listing> ListingsCollection;

        static MongoListingRepository()
        {
            Server = Client.GetServer();
            Db = Server.GetDatabase("classifieds");
            ListingsCollection = Db.GetCollection<Listing>("classifieds");
        }

        public Listing GetById(string listingId, string appId, bool includeDrafts, bool increaseViewCounter)
        {
            var query = Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId));
            if (!includeDrafts)
            {
                query = Query.And(query, Query<Listing>.EQ(x => x.IsPublished, true));
            }
            if (increaseViewCounter)
            {
                var listing = ListingsCollection.FindAndModify(query, null, Update<Listing>.Inc(x => x.ViewCount, 1), true);
                return listing.GetModifiedDocumentAs<Listing>();
            }
            else
            {
                var listing = ListingsCollection.FindOne(query);
                return listing;
            }   
        }

        public IList<Listing> GetByProfileId(string appId, string profileId, bool includeDrafts)
        {
            var query = Query.And(
                    Query<Listing>.EQ(x => x.ProfileId, profileId),
                    Query<Listing>.EQ(x => x.AppId, appId));
            if (!includeDrafts)
            {
                query = Query.And(query, Query<Listing>.EQ(x => x.IsPublished, true));
            }

            // now get the listings
            var listings = ListingsCollection.Find(query);
            return listings.ToList();
        }

        public string Insert(Listing listing)
        {
            try
            {
                ListingsCollection.Insert(listing);
                return listing.Id;
            }
            catch(MongoException mex)
            {
                throw;
            }
        }

        public void Update(Listing listing)
        {
            try
            {
                var query = Query.And(
                    Query<Listing>.EQ(x => x.Id, listing.Id),
                    Query<Listing>.EQ(x => x.AppId, listing.AppId));
                var update = new UpdateBuilder<Listing>()
                    .Set(x => x.Title, listing.Title)
                    .Set(x => x.Content, listing.Content);
                if (listing.ContactInfo != null) update.Set(x => x.ContactInfo, listing.ContactInfo);
                if (listing.Pricing != null) update.Set(x => x.Pricing, listing.Pricing);
                if (listing.SchedulingTemplate != null) update.Set(x => x.SchedulingTemplate, listing.SchedulingTemplate);
                if (listing.Metadata != null) update.Set(x => x.Metadata, listing.Metadata);

                ListingsCollection.FindAndModify(query, null, update);
            }
            catch(MongoException mex)
            {
                throw;
            }
        }

        public void AddExternalMedia(string listingId, string appId, IList<MediaFile> media)
        {
            try
            {
                ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), Update<Listing>.PushAll<MediaFile>(x => x.ExternalMedia, media));
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void DeleteExternalMedia(string listingId, string appId, string url)
        {
            try
            {
                ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), MongoDB.Driver.Builders.Update.Pull("ExternalMedia", Query.EQ ("Url", url)));
            }
            catch (MongoException mex)
            {
                throw;
            }
        }


        public void Publish(string listingId, string appId)
        {
            try
            {
                ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), Update<Listing>.Set(x => x.IsPublished, true));
            }
            catch (MongoException mex)
            {
                throw;
            }
        }


        public void IncreaseCommentCounter(string listingId, string appId, int value)
        {
            try
            {
                ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), Update<Listing>.Inc(x => x.CommentCount, value));
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void IncreaseCounter(string listingId, string appId, ListingCounters counters, int value)
        {
            try
            {
                var query = Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)
                    );
                var update = new UpdateBuilder<Listing>();
                if (counters.HasFlag(ListingCounters.Comments)) update.Inc(x => x.CommentCount, value);
                if (counters.HasFlag(ListingCounters.Favorites)) update.Inc(x => x.FavoriteCount, value);
                if (counters.HasFlag(ListingCounters.Flags)) update.Inc(x => x.FlagCount, value);
                if (counters.HasFlag(ListingCounters.Views)) update.Inc(x => x.ViewCount, value);
                if (counters.HasFlag(ListingCounters.Clicks)) update.Inc(x => x.ClickCount, value);
                if (counters.HasFlag(ListingCounters.Bookings)) update.Inc(x => x.BookingCount, value);
                if (counters.HasFlag(ListingCounters.Purchases)) update.Inc(x => x.PurchaseCount, value);
                ListingsCollection.Update(query, update);
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void IncreaseFavoriteCounter(string listingId, string appId, int value)
        {
            try
            {
                ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), Update<Listing>.Inc(x => x.FavoriteCount, value));
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void AddHashtags(string listingId, string appId, string[] hashtags)
        {
            try
            {
                ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), Update<Listing>.AddToSetEach<string>(x => x.Hashtags, hashtags));
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void RemoveHashtags(string listingId, string appId, string[] hashtags)
        {
            throw new NotImplementedException();
        }

        public IList<Listing> Search(string tag, IEnumerable<CustomAttribute> metadata, double? priceMin, double? priceMax, Location location, string appId, bool includeDrafts, bool increaseViewCounter)
        {
            var queries = new List<IMongoQuery>() {
                Query<Listing>.EQ(x => x.AppId, appId)
            };
            if (!string.IsNullOrEmpty(tag))
            {
                queries.Add(Query<Listing>.In(x => x.Hashtags, new string[] { tag }));
            }
            if (metadata != null)
            {
                foreach (var m in metadata)
                {
                    queries.Add(Query.ElemMatch("Metadata", Query.And(Query.EQ("Key", m.Key), Query.EQ("Value", m.Value))));
                }
            }
            if (priceMin.HasValue)
            {
                queries.Add(Query<Listing>.GTE(x => x.Pricing.Price, priceMin));
            }
            if (priceMax.HasValue)
            {
                queries.Add(Query<Listing>.LTE(x => x.Pricing.Price, priceMax));
            }
            if (location != null)
            {
                ListingsCollection.EnsureIndex(IndexKeys.GeoSpatial("Location"));
                queries.Add(Query<Listing>.Near(x => x.ContactInfo.Location, location.Longitude, location.Latitude, 1/111.12, true));
            }
            if (!includeDrafts)
            {
                queries.Add(Query<Listing>.EQ(x => x.IsPublished, true));
            }

            var query = Query.And(queries);
            // increase the view count of all deals
            if (increaseViewCounter) ListingsCollection.Update(query, Update<Listing>.Inc(x => x.ViewCount, 1), UpdateFlags.Multi);
            // now get the listings
            var listings = ListingsCollection.Find(query);
            return listings.ToList();
        }
    }
}