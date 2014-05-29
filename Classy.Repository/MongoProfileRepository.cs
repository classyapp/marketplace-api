using Classy.Models;
using Classy.Repository.Infrastructure;
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

        public MongoProfileRepository(MongoDatabaseProvider db)
        {
            ProfilesCollection = db.GetCollection<Profile>();
            ProxyClaimsCollection = db.GetCollection<ProxyClaim>();
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

        public Profile GetByUsername(string appId, string username, bool increaseViewCounter, string culture)
        {
            var query = Query<Profile>.Where(x => x.UserName == username && x.AppId == appId);
            Profile profile = null;

            if (increaseViewCounter)
            {
                var document = ProfilesCollection.FindAndModify(query, null, Update<Profile>.Inc(x => x.ViewCount, 1), true);
                profile = document.GetModifiedDocumentAs<Profile>();
            }
            else
            {
                profile = ProfilesCollection.FindOne(query);
            }

            if (profile != null) profile.Translate(culture);
            return profile;
        }


        public Profile GetById(string appId, string profileId, bool increaseViewCounter, string culture)
        {
            var query = Query<Profile>.Where(x => x.Id == profileId && x.AppId == appId);
            Profile profile = null;
            if (increaseViewCounter)
            {
                var document = ProfilesCollection.FindAndModify(query, null, Update<Profile>.Inc(x => x.ViewCount, 1), true);
                profile = document.GetModifiedDocumentAs<Profile>();
            }
            else
            {
                profile = ProfilesCollection.FindOne(query);
            }

            if (profile != null) profile.Translate(culture);
            return profile;
        }

        public IList<Profile> GetByIds(string appId, string[] profileIds, string culture)
        {
            var profiles = ProfilesCollection.Find(Query.And(
                Query<Profile>.Where(x => x.AppId == appId),
                Query<Profile>.In(x => x.Id, new BsonArray(profileIds))));

            foreach (var profile in profiles)
            {
                profile.Translate(culture);
            }

            return profiles.ToList();
        }


        public void IncreaseCounter(string appId, string profileId, ProfileCounters counters, int value)
        {
            var query = Query<Profile>.Where(x => x.Id == profileId && x.AppId == appId);
            var update = new UpdateBuilder<Profile>();
            if (counters.HasFlag(ProfileCounters.Listing)) update.Inc(x => x.ListingCount, value);
            if (counters.HasFlag(ProfileCounters.Followers)) update.Inc(x => x.FollowerCount, value);
            if (counters.HasFlag(ProfileCounters.Following)) update.Inc(x => x.FollowingCount, value);
            if (counters.HasFlag(ProfileCounters.Comments)) update.Inc(x => x.CommentCount, value);
            if (counters.HasFlag(ProfileCounters.Reviews)) update.Inc(x => x.ReviewCount, value);
            if (counters.HasFlag(ProfileCounters.Rank)) update.Inc(x => x.Rank, value);
            ProfilesCollection.Update(query, update);
        }

        public IList<Profile> Search(string appId, string searchQuery, string category, Location location, IDictionary<string, string> metadata,
            bool professionalsOnly, bool ignoreLocation, int page, int pageSize, ref long count, string culture)
        {

            var sort = SortBy.Ascending("ProfessionalInfo.IsProxy").Descending("Languages." + culture, "Rank", "Username");

            #region Build queries for match
            var queries = new List<IMongoQuery>() {
                Query<Profile>.EQ(x => x.AppId, appId),
                // exclude admin accoutns
                Query<Profile>.NE(x => x.Permissions, "admin")
            };

            if (professionalsOnly)
            {
                queries.Add(Query.NE("ProfessionalInfo", BsonNull.Value));
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // escape 
                searchQuery = searchQuery.Replace("?", "\\?");
                // search for professional with matching company name, contact name, or website
                var nameQuery = Query.Or(
                    Query<Profile>.Matches(x => x.ProfessionalInfo.CompanyContactInfo.FirstName, new BsonRegularExpression(new Regex(searchQuery, RegexOptions.IgnoreCase))),
                    Query<Profile>.Matches(x => x.ProfessionalInfo.CompanyContactInfo.LastName, new BsonRegularExpression(new Regex(searchQuery, RegexOptions.IgnoreCase))),
                    Query<Profile>.Matches(x => x.ProfessionalInfo.CompanyName, new BsonRegularExpression(new Regex(searchQuery, RegexOptions.IgnoreCase))),
                    Query<Profile>.Matches(x => x.ProfessionalInfo.CompanyContactInfo.WebsiteUrl, new BsonRegularExpression(new Regex(searchQuery, RegexOptions.IgnoreCase)))
                    );
                if (!professionalsOnly)
                    nameQuery = Query.Or(
                        nameQuery,
                        // search in contact info of user 
                        Query<Profile>.Matches(x => x.ContactInfo.FirstName, new BsonRegularExpression(new Regex(searchQuery, RegexOptions.IgnoreCase))),
                        Query<Profile>.Matches(x => x.ContactInfo.LastName, new BsonRegularExpression(new Regex(searchQuery, RegexOptions.IgnoreCase)))
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
                    queries.Add(Query.EQ(string.Concat("Metadata.", m.Key), m.Value));
                }
            }

            IMongoQuery locationQueryByGPS = null;
            IMongoQuery locationByAddress = null;
            if (!ignoreLocation)
            {
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
                            locationByAddress = Query<Profile>.EQ(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.Country, location.Address.Country);
                            if (!string.IsNullOrEmpty(location.Address.City)) locationByAddress = Query.And(locationByAddress, Query<Profile>.EQ(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.City, location.Address.City));
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
                            locationByAddress = Query<Profile>.EQ(x => x.ContactInfo.Location.Address.Country, location.Address.Country);
                            if (!string.IsNullOrEmpty(location.Address.City)) locationByAddress = Query.And(locationByAddress, Query<Profile>.EQ(x => x.ContactInfo.Location.Address.City, location.Address.City));
                        }
                    }
                }
                if (locationQueryByGPS != null)
                {
                    queries.Add(locationQueryByGPS);
                }
                else if (locationByAddress != null)
                {
                    queries.Add(locationByAddress);
                }
            }
            #endregion

            // try query, and redo for entire country if nothing found nearby
            var query = Query.And(queries);
            MongoCursor<Profile> profiles = null;
            if (ProfilesCollection.Count(query) == 0)
            {
                if (locationQueryByGPS != null && locationByAddress != null)
                {
                    queries.Remove(locationQueryByGPS);
                    queries.Add(locationByAddress);
                    query = Query.And(queries);
                }
            }

            if (page <= 0)
            {
                profiles = ProfilesCollection.Find(query).SetSortOrder(sort);
            }
            else
            {
                profiles = ProfilesCollection.Find(query).SetSortOrder(sort).SetSkip((page - 1) * pageSize).SetLimit(pageSize);
            }
            count = ProfilesCollection.Count(query);

            foreach (var profile in profiles)
            {
                profile.Translate(culture);
            }

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

        public IList<string> GetDistinctCitiesByCountry(string appId, string countryCode)
        {
            return ProfilesCollection.Distinct<string>(
                "ProfessionalInfo.CompanyContactInfo.Location.Address.City",
                Query<Profile>.Where(
                    x => x.AppId == appId &&
                        x.ProfessionalInfo.CompanyContactInfo.Location.Address.Country == countryCode &&
                        !string.IsNullOrEmpty(x.ProfessionalInfo.CompanyContactInfo.Location.Address.City)
                    )).ToList();
        }

        public Profile GetByEmailHash(string appId, string hash)
        {
            return ProfilesCollection.FindOne(Query.And(Query.EQ("AppId", appId), Query.EQ("Metadata." + Profile.EmailHashMetadata, hash)));
        }
    }
}
