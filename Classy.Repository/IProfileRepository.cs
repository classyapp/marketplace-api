﻿using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    [Flags]
    public enum ProfileCounters
    {
        None = 0,
        Followers = 1,
        Following = 2,
        Rank = 4,
        Listing = 8,
        Comments = 16,
        Views = 32,
        Reviews = 64
    }

    public interface IProfileRepository
    {
        string Save(Profile profile);
        Profile GetById(string appId, string profileId, bool increaseViewCounter, string culture);
        IList<Profile> GetByIds(string appId, string[] profileIds, string culture);
        Profile GetByUsername(string appId, string username, bool increaseViewCounter, string culture);
        IList<Profile> Search(string appId, string searchQuery, string category, Location location, IDictionary<string, string> metadata, bool professionalsOnly, bool ignoreLocation, int page, int pageSize, ref long count, string culture);
        void Delete(string profileId);
        void IncreaseCounter(string appId, string profileId, ProfileCounters counters, int value); // returns the profile id or null
        // proxies
        string SaveProxyClaim(ProxyClaim claim);
        ProxyClaim GetProxyClaimById(string appId, string claimId);
        // cities (for localization)
        IList<string> GetDistinctCitiesByCountry(string appId, string countryCode);
        Profile GetByEmailHash(string appId, string hash);
    }
}
