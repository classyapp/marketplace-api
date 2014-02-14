using Classy.Models;
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
        Profile GetById(string appId, string profileId, bool increaseViewCounter);
        IList<Profile> GetByIds(string appId, string[] profileIds);
        Profile GetByUsername(string appId, string username, bool increaseViewCounter);
        IList<Profile> Search(string appId, string displayName, string category, Location location, IDictionary<string, string> metadata, bool professionalsOnly, int page, int pageSize, ref long count);
        void Delete(string profileId);
        void IncreaseCounter(string appId, string profileId, ProfileCounters counters, int value); // returns the profile id or null
        // proxies
        string SaveProxyClaim(ProxyClaim claim);
        ProxyClaim GetProxyClaimById(string appId, string claimId);
    }
}
