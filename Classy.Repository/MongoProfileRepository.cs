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
        private MongoCollection<Profile> ProfilesCollection;
        private MongoCollection<ProxyClaim> ProxyClaimsCollection;

        public MongoProfileRepository(MongoDatabase db)
        {
            ProfilesCollection = db.GetCollection<Profile>("profiles");
            ProxyClaimsCollection = db.GetCollection<ProxyClaim>("proxyclaims");
        }

        public string Save(Profile profile)
        {
            try
            {
                ProfilesCollection.Save(profile);
                return profile.Id;
            }
            catch (Exception ex)
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

        public IList<Profile> Search(string appId, string displayName, string category, Location location, IDictionary<string, string> metadata, 
            bool professionalsOnly, int page, int pageSize, ref long count)
        {
            // sort order
            var sortOrder = SortBy<Profile>.Descending(x => x.Rank, x => x.UserName);

            // query building
            var queries = new List<IMongoQuery>() {
                Query<Profile>.EQ(x => x.AppId, appId)
            };
            if (professionalsOnly)
            {
                queries.Add(Query.NE("ProfessionalInfo", BsonNull.Value));
            }
            if (!string.IsNullOrEmpty(displayName))
            {
                // search for professional with matching company name, or contact name
                var nameQuery = Query.Or(
                    Query<Profile>.Matches(x => x.ProfessionalInfo.CompanyContactInfo.FirstName, BsonRegularExpression.Create(new Regex(displayName, RegexOptions.IgnoreCase))),
                    Query<Profile>.Matches(x => x.ProfessionalInfo.CompanyContactInfo.LastName, BsonRegularExpression.Create(new Regex(displayName, RegexOptions.IgnoreCase))),
                    Query<Profile>.Matches(x => x.ProfessionalInfo.CompanyName, BsonRegularExpression.Create(new Regex(displayName, RegexOptions.IgnoreCase)))
                    );
                if (!professionalsOnly)
                    nameQuery = Query.Or(
                        nameQuery,
                        // search in contact info of user 
                        Query<Profile>.Matches(x => x.ContactInfo.FirstName, BsonRegularExpression.Create(new Regex(displayName, RegexOptions.IgnoreCase))),
                        Query<Profile>.Matches(x => x.ContactInfo.LastName, BsonRegularExpression.Create(new Regex(displayName, RegexOptions.IgnoreCase)))
                );
                queries.Add(nameQuery);
            }
            if (!string.IsNullOrEmpty(category))
            {
                queries.Add(Query<Profile>.EQ(x => x.ProfessionalInfo.Category, category));
            }
            if (metadata != null)
            {
                foreach (var m in metadata)
                {
                    queries.Add(Query.EQ(string.Concat("Metadata", m.Key), m.Value));
                }
            }
            IMongoQuery locationQueryByGPS = null;
            IMongoQuery locationByCountry = null;
            if (location != null)
            {
                if (professionalsOnly)
                {
                    if (location.Coords != null)
                    {
                        ProfilesCollection.EnsureIndex(IndexKeys.GeoSpatial("ProfessionalInfo.CompanyContactInfo.Location.Coords"));
                        locationQueryByGPS = Query<Profile>.Near(x => x.ProfessionalInfo.CompanyContactInfo.Location.Coords, location.Coords.Longitude.Value, location.Coords.Latitude.Value, 1 / 111.12, true);
                    }
                    if (location.Address != null && !string.IsNullOrEmpty(location.Address.Country))
                    {
                        locationByCountry = Query<Profile>.EQ(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.Country, location.Address.Country);
                    }
                }
                else
                {
                    if (location.Coords != null)
                    {
                        ProfilesCollection.EnsureIndex(IndexKeys.GeoSpatial("ContactInfo.Location.Coords"));
                        locationQueryByGPS = Query<Profile>.Near(x => x.ContactInfo.Location.Coords, location.Coords.Longitude.Value, location.Coords.Latitude.Value, 1 / 111.12, true);
                    }
                    if (location.Address != null && !string.IsNullOrEmpty(location.Address.Country))
                    {
                        locationByCountry = Query<Profile>.EQ(x => x.ContactInfo.Location.Address.Country, location.Address.Country);
                    }
                }
            }
            if (locationQueryByGPS != null)
            {
                queries.Add(locationQueryByGPS);
            }
            else if (locationByCountry != null)
            {
                queries.Add(locationByCountry);
            }

            // try query, and redo for entire country if nothing found nearby
            var query = Query.And(queries);
            MongoCursor<Profile> profiles = null;
            if (ProfilesCollection.Count(query) == 0)
            {
                if (locationQueryByGPS != null && locationByCountry != null)
                {
                    queries.Remove(locationQueryByGPS);
                    queries.Add(locationByCountry);
                    query = Query.And(queries);
                }
            }
            if (page <= 0)
            {   
                profiles = ProfilesCollection.Find(query).SetSortOrder(sortOrder);
            }
            else
            {
                profiles =  ProfilesCollection.Find(query).SetSortOrder(sortOrder).SetSkip((page - 1) * pageSize).SetLimit(pageSize);
            }
            count = ProfilesCollection.Count(query);

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
