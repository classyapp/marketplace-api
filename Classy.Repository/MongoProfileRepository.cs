using Classy.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Classy.Repository
{
    public class MongoProfileRepository : IProfileRepository
    {
        static MongoClient Client = new MongoClient("mongodb://localhost");
        static MongoServer Server;
        static MongoDatabase Db;
        static MongoCollection<Profile> ProfilesCollection;
        static MongoCollection<ProxyClaim> ProxyClaimsCollection;

        static MongoProfileRepository()
        {
            Server = Client.GetServer();
            Db = Server.GetDatabase("classifieds");
            ProfilesCollection = Db.GetCollection<Profile>("profiles");
            ProxyClaimsCollection = Db.GetCollection<ProxyClaim>("proxyclaims");   
        }

        public string Save(Profile profile)
        {
            try
            {
                ProfilesCollection.Save(profile);
                return profile.Id;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public Profile GetByUsername(string appId, string username, bool increaseViewCounter)
        {
            var query = Query<Profile>.Where(x => x.UserName == username && x.AppId == appId);
            if (increaseViewCounter)
            {
                var profile = ProfilesCollection.FindAndModify(query, null, Update<Profile>.Inc(x => x.ViewCount, 1), true);
                return profile.GetModifiedDocumentAs<Profile>();
            }
            else
            {
                var listing = ProfilesCollection.FindOne(query);
                return listing;
            }
        }


        public Profile GetById(string appId, string profileId, bool increaseViewCounter)
        {
            var query = Query<Profile>.Where(x => x.Id == profileId && x.AppId == appId);
            if (increaseViewCounter)
            {
                var result = ProfilesCollection.FindAndModify(query, null, Update<Profile>.Inc(x => x.ViewCount, 1), true);
                return result.GetModifiedDocumentAs<Profile>();
            }
            else
            {
                var profile = ProfilesCollection.FindOne(query);
                return profile;
            }   
        }

        public IList<Profile> GetByIds(string appId, string[] profileIds)
        {
            var profiles = ProfilesCollection.Find(Query.And(
                Query<Profile>.Where(x => x.AppId == appId),
                Query<Profile>.In(x => x.Id, new BsonArray(profileIds))));
            return profiles.ToList();
        }


        public void IncreaseCounter(string appId, string profileId, ProfileCounters counters, int value)
        {
            var query = Query<Profile>.Where(x => x.Id == profileId && x.AppId == appId);
            var update = new UpdateBuilder<Profile>();
            if (counters.HasFlag(ProfileCounters.Listing)) update.Inc(x => x.ListingCount, value);
            if (counters.HasFlag(ProfileCounters.Followers)) update.Inc(x => x.FollowerCount, value);
            if (counters.HasFlag(ProfileCounters.Following)) update.Inc(x => x.FollowingCount, value);
            if (counters.HasFlag(ProfileCounters.Rank)) update.Inc(x => x.Rank, value);
            if (counters.HasFlag(ProfileCounters.Comments)) update.Inc(x => x.CommentCount, value);
            if (counters.HasFlag(ProfileCounters.Reviews)) update.Inc(x => x.ReviewCount, value);
            ProfilesCollection.Update(query, update);
        }

        public IList<Profile> Search(string appId, string displayName, string category, Location location, IDictionary<string, string> metadata)
        {
            var queries = new List<IMongoQuery>() {
                Query<Profile>.EQ(x => x.AppId, appId)
            };
            if (!string.IsNullOrEmpty(displayName))
            {
                var nameQuery = Query.Or(
                    // search in contact info of user 
                    Query<Profile>.Matches(x => x.ContactInfo.Name, BsonRegularExpression.Create(new Regex(displayName))),
                    // search in contact info of seller (for business entities)
                    Query<Profile>.Matches(x => x.SellerInfo.ContactInfo.Name, BsonRegularExpression.Create(new Regex(displayName))),
                    // search in legal entity name
                    Query<Profile>.Matches(x => x.SellerInfo.LegalEntityName, BsonRegularExpression.Create(new Regex(displayName))),
                    // search in seller display name
                    Query<Profile>.Matches(x => x.SellerInfo.DisplayName, BsonRegularExpression.Create(new Regex(displayName)))
                );
                queries.Add(nameQuery);
            }
            if (!string.IsNullOrEmpty(category))
            {
                queries.Add(Query<Profile>.EQ(x => x.SellerInfo.Category, category));
            }
            if (metadata != null)
            {
                foreach (var m in metadata)
                {
                    queries.Add(Query.ElemMatch("Metadata", Query.And(Query.EQ("Key", m.Key), Query.EQ("Value", m.Value))));
                }
            }
            if (location != null)
            {
                //ProfilesCollection.EnsureIndex(IndexKeys.GeoSpatial("merchant location"));
                //queries.Add(Query<Profile>.Near(x => x.ContactInfo.Location, location.Longitude, location.Latitude, 1 / 111.12, true));
            }

            var query = Query.And(queries);
           var profiles = ProfilesCollection.Find(query);
            return profiles.ToList();
        }

        public void Delete(string profileId)
        {
            try
            {
                ProfilesCollection.Remove(Query<Profile>.EQ(x => x.Id, profileId));
            }
            catch (MongoException mex)
            { 
                throw mex; 
            }
        }

        public string SaveProxyClaim(ProxyClaim claim)
        {
            ProxyClaimsCollection.Save(claim);
            return claim.Id;
        }

        public ProxyClaim GetProxyClaimById(string appId, string claimId)
        {
            return ProxyClaimsCollection.FindOne(Query<ProxyClaim>.Where(x => x.AppId == appId && x.Id == claimId));
        }
    }
}
