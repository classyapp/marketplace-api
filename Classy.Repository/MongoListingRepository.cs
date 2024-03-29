using classy.DTO.Request;
using Classy.Repository.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Builders;
using Classy.Models;
using System.Text.RegularExpressions;

namespace Classy.Repository
{
    public class MongoListingRepository : IListingRepository
    {
        private readonly MongoCollection<Listing> ListingsCollection;

        public MongoListingRepository(MongoDatabaseProvider db)
        {
            ListingsCollection = db.GetCollection<Listing>();
        }

        public Listing GetById(string listingId, string appId, bool includeDrafts, string culture)
        {
            var listings = GetById(new[] { listingId }, appId, includeDrafts, culture);
            if (listings == null || !listings.Any()) return null;
            return listings[0];
        }

        public IList<Listing> GetById(string[] listingId, string appId, bool includeDrafts, string culture)
        {
            var query = Query.And(
                    Query<Listing>.In(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId));
            if (!includeDrafts)
            {
                query = Query.And(query, Query<Listing>.EQ(x => x.IsPublished, true));
            }
            var listings = ListingsCollection.Find(query);
            foreach (var listing in listings)
            {
                listing.Translate(culture);
            }
            return listings.ToList(); 
        }

        public IList<Listing> GetByProfileId(string appId, string profileId, bool includeDrafts, string culture)
        {
            var query = Query.And(
                    Query<Listing>.EQ(x => x.ProfileId, profileId),
                    Query<Listing>.EQ(x => x.AppId, appId));
            if (!includeDrafts)
            {
                query = Query.And(query, Query<Listing>.EQ(x => x.IsPublished, true));
            }

            // now get the listings
            MongoCursor<Listing> listings = ListingsCollection.Find(query);
            foreach (var listing in listings)
            {
                listing.Translate(culture);
            }
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
                    .Set(x => x.ProfileId, listing.ProfileId)
                    .Set(x => x.Title, listing.Title)
                    .Set(x => x.Content, listing.Content)
                    .Set(x => x.Translations, listing.Translations);
                if (listing.ContactInfo != null) update.Set(x => x.ContactInfo, listing.ContactInfo);
                if (listing.PricingInfo != null) update.Set(x => x.PricingInfo, listing.PricingInfo);
                if (listing.SchedulingTemplate != null) update.Set(x => x.SchedulingTemplate, listing.SchedulingTemplate);
                update.Set(x => x.Metadata, listing.Metadata);
                update.Set(x => x.Hashtags, listing.Hashtags);
                update.Set(x => x.TranslatedKeywords, listing.TranslatedKeywords);
                if (listing.TranslatedKeywords != null)
                    update.Set(x => x.SearchableKeywords, listing.TranslatedKeywords.SelectMany(s => s.Value));
                else
                    update.Set(x => x.SearchableKeywords, null);
              

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

        public void UpdateExternalMedia(string listingId, string appId, MediaFile media)
        {
            try
            {
                var query = Query.And(
                   Query<Listing>.EQ(x => x.Id, listingId),
                   Query<Listing>.EQ(x => x.AppId, appId),
                   Query.EQ("ExternalMedia.Key", media.Key)
                );

                var result = ListingsCollection.Update(
                    query,
                    MongoDB.Driver.Builders.Update.Set("ExternalMedia.$", media.ToBsonDocument())
                );
            }
            catch
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

        public void Delete(string listingId, string appId)
        {
            try
            {
                ListingsCollection.Remove(Query<Listing>.Where(x => x.Id == listingId && x.AppId == appId));
            }
            catch (MongoException mex)
            {
                throw;
            }        
        }

        public void IncreaseCounter(string listingId, string appId, ListingCounters counters, int value)
        {
            IncreaseCounter(new string[] { listingId }, appId, counters, value);
        }

        public void IncreaseCounter(string[] listingId, string appId, ListingCounters counters, int value)
        {
            try
            {
                var query = Query.And(
                    Query<Listing>.In(x => x.Id, listingId),
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
                if (counters.HasFlag(ListingCounters.AddToCollection)) update.Inc(x => x.AddToCollectionCount, value);
                if (counters.HasFlag(ListingCounters.DisplayOrder)) update.Inc(x => x.DisplayOrder, value);
                ListingsCollection.Update(query, update, new MongoUpdateOptions { Flags = UpdateFlags.Multi });
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

        public IList<Listing> Search(string Q, string[] tags, string[] categories, string[] listingTypes, IDictionary<string, string[]> metadata,
            IDictionary<string, string[]> objectQuery, double? priceMin, double? priceMax, Location location, string appId,
            bool includeDrafts, bool increaseViewCounter, int page, int pageSize, ref long count, SortMethod sortMethod, string culture)
        {
            // set sort order
            var sortOrder = sortMethod == SortMethod.Date
                ? SortBy<Listing>.Descending(x => x.Created)
                : SortBy<Listing>.Descending(x => x.EditorsRank)
                    .Descending(x => x.DisplayOrder)
                    .Descending(x => x.FavoriteCount);

            // app id
            var queries = new List<IMongoQuery>() {
                Query<Listing>.EQ(x => x.AppId, appId)
            };

            // object query
            if (objectQuery != null)
            {
                foreach (var key in objectQuery.Keys)
                {
                    if (objectQuery[key].Length == 1)
                        queries.Add(Query.EQ(key, objectQuery[key][0]));
                    else
                        queries.Add(Query.In(key, objectQuery[key].Select(v => new BsonString(v))));
                }
            }

            // free search query
            if (!string.IsNullOrEmpty(Q))
            {
                var titleQuery = Query.Matches("Title", new BsonRegularExpression("\\b" + Q + "\\w*\\b"));
                var contentQuery = Query.Matches("Content", new BsonRegularExpression("\\b" + Q + "\\w*\\b"));
                queries.Add(Query.Or(titleQuery, contentQuery));
            }

            // listing types
            if (listingTypes != null && listingTypes.Any())
            {
                var listingTypesQueries = new List<IMongoQuery>();
                foreach (var listingType in listingTypes)
                {
                    listingTypesQueries.Add(Query<Listing>.EQ(x => x.ListingType, listingType));
                }
                queries.Add(Query.Or(listingTypesQueries));
            }

            // tags
            if (tags != null && tags.Any())
            {
                var tagQueries = new List<IMongoQuery>();
                tagQueries.Add(Query.In("Hashtags", tags.Select(x => new BsonRegularExpression(new Regex(x, RegexOptions.IgnoreCase | RegexOptions.Compiled)))));
                tagQueries.Add(Query.In("SearchableKeywords", tags.Select(x => new BsonRegularExpression(new Regex(x, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)))));
                queries.Add(Query.Or(tagQueries));
            }

            // categories
            if (categories != null && categories.Any(x => !string.IsNullOrEmpty(x)))
            {
                queries.Add(Query.In("Categories",
                    categories.Where(x => !string.IsNullOrEmpty(x)).Select(
                        x => new BsonRegularExpression(new Regex(x, RegexOptions.IgnoreCase | RegexOptions.Compiled)))));
            }

            // metadata
            if (metadata != null && metadata.Any())
            {
                foreach (var m in metadata)
                {
                    if (m.Value.Count() == 1)
                    {
                        queries.Add(Query.EQ(string.Concat("Metadata.", m.Key), m.Value[0]));
                    }
                    else
                    {
                        var mQueries = new List<IMongoQuery>();
                        foreach (var s in m.Value)
                        {
                            mQueries.Add(Query.EQ(string.Concat("Metadata.", m.Key), s));
                        }
                        queries.Add(Query.Or(mQueries));
                    }
                }
            }

            // price range
            if (priceMin.HasValue)
            {
                queries.Add(Query.ElemMatch("PricingInfo.PurchaseOptions", Query.GTE("Price", priceMin)));
            }
            if (priceMax.HasValue)
            {
                queries.Add(Query.ElemMatch("PricingInfo.PurchaseOptions", Query.LTE("Price", priceMax)));
            }

            // geo
            if (location != null)
            {
                //ListingsCollection.EnsureIndex(IndexKeys.GeoSpatial("Location"));
                //queries.Add(Query<Listing>.Near(x => x.ContactInfo.Location, location.Longitude.Value, location.Latitude.Value, 1/111.12, true));
            }

            // including drafts
            if (!includeDrafts)
            {
                queries.Add(Query<Listing>.EQ(x => x.IsPublished, true));
            }

            // add them all up
            var query = Query.And(queries);
            // now get the listings
            MongoCursor<Listing> listings = null;
            if (page <= 0)
            {
                // increase the view count of all deals
                if (increaseViewCounter) ListingsCollection.Update(query, Update<Listing>.Inc(x => x.ViewCount, 1), UpdateFlags.Multi);
                listings = ListingsCollection.Find(query).SetSortOrder(sortOrder);
                count = listings.Count();
            }
            else
            {
                listings = ListingsCollection.Find(query).SetSortOrder(sortOrder).SetSkip((page - 1) * pageSize).SetLimit(pageSize);
                count = ListingsCollection.Count(query);
                var ids = listings.Select(l => l.Id).ToArray();
                if (increaseViewCounter) ListingsCollection.Update(Query<Listing>.Where(l => ids.Contains(l.Id)), Update<Listing>.Inc(x => x.ViewCount, 1), UpdateFlags.Multi);
            }

            foreach (var listing in listings)
            {
                listing.Translate(culture);
            }

            return listings.ToList();
        }

        public void SetListingErrorForMediaFile(string key, string error)
        {
            MongoCursor<Listing> listings = ListingsCollection.Find(Query.ElemMatch("ExternalMedia", Query.EQ("Key", key)));
            Listing listing = listings.FirstOrDefault();
            if (listing != null)
            {
                listing.Errors = error;
                ListingsCollection.Save(listing);
            }
        }

        public void EditMultipleListings(string[] ids, int? editorsRank, string appId, Dictionary<string, string> metadata)
        {
            var updateBuilder = new UpdateBuilder<Listing>();
            if (editorsRank.HasValue)
                updateBuilder.Set(x => x.EditorsRank, editorsRank);

            var metadataUpdater = new UpdateBuilder();
            foreach (var listingInfo in metadata)
                metadataUpdater.Set("Metadata." + listingInfo.Key, new BsonString(listingInfo.Value));

            ListingsCollection.Update(
                Query.And(Query<Listing>.In(x => x.Id, ids), Query<Listing>.EQ(x => x.AppId, appId)),
                updateBuilder.Combine(metadataUpdater), UpdateFlags.Multi);
        }


        public Listing GetBySKU(string sku)
        {
            return ListingsCollection.FindOne(Query<Listing>.ElemMatch(x => x.PricingInfo.PurchaseOptions, y => y.EQ(p => p.SKU, sku)));
        }


        public void DeleteExternalMediaById(string listingId, string appId, string key)
        {
            ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), MongoDB.Driver.Builders.Update.Pull("ExternalMedia", Query.EQ("Key", key)));

            ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), MongoDB.Driver.Builders.Update.Pull("PricingInfo.BaseOption.MediaFiles", Query.EQ("Key", key)));

            ListingsCollection.Update(Query.And(
                    Query<Listing>.EQ(x => x.Id, listingId),
                    Query<Listing>.EQ(x => x.AppId, appId)), MongoDB.Driver.Builders.Update.Pull("PricingInfo.PurchaseOption.MediaFiles", Query.EQ("Key", key)));
        }

        public List<string> CheckDuplicateSKUs(string appId, string profileId, string listingId, string[] skus)
        {
            IMongoQuery query = null;
            if (string.IsNullOrWhiteSpace(listingId))
            {
                query = Query.And(Query<Listing>.EQ(l => l.AppId, appId), Query<Listing>.EQ(l => l.ProfileId, profileId),
                         Query.Or(Query<Listing>.In(l => l.PricingInfo.BaseOption.SKU, skus),
                                  Query<Listing>.ElemMatch(l => l.PricingInfo.PurchaseOptions, po => po.In(i => i.SKU, skus))));
            }
            else
            {
                query = Query.And(Query<Listing>.EQ(l => l.AppId, appId), Query<Listing>.EQ(l => l.ProfileId, profileId), Query<Listing>.NE(l => l.Id, listingId),
                                        Query.Or(Query<Listing>.In(l => l.PricingInfo.BaseOption.SKU, skus),
                                                 Query<Listing>.ElemMatch(l => l.PricingInfo.PurchaseOptions, po => po.In(i => i.SKU, skus))));
            }

            var listings = ListingsCollection.Find(query);
            if (listings.Count() > 0)
            {
                List<string> duplicates = new List<string>();

                duplicates.AddRange(listings.Where(l => skus.Contains(l.PricingInfo.BaseOption.SKU)).Select(l => l.PricingInfo.BaseOption.SKU).ToArray());
                foreach (var list in listings.Select(l => (l.PricingInfo.PurchaseOptions != null ? l.PricingInfo.PurchaseOptions.Select(po => po.SKU) : null)).ToArray())
                {
                    if (list != null)
                    {
                        duplicates.AddRange(list.Where(l => skus.Contains(l)));
                    }
                }

                return duplicates;
            }

            return new List<string>();
        }
    }
}