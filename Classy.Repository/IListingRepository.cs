using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace Classy.Repository
{
    [Flags]
    public enum ListingCounters
    {
        None = 0,
        Comments = 1,
        Flags = 2,
        Favorites = 4,
        Views = 8,
        Clicks = 16,
        Bookings = 32,
        Purchases = 64,
        AddToCollection = 128
    }

    public interface IListingRepository
    {
        Listing GetById(string listingId, string appId, bool includeDrafts);
        IList<Listing> GetById(string[] listingId, string appId, bool includeDrafts);
        IList<Listing> GetByProfileId(string appId, string profileId, bool includeDrafts, int page);
        IList<Listing> Search(string tag, string listingType, IDictionary<string, string> metadata, double? priceMin, 
            double? priceMax, Location location, string appId, bool includeDrafts, bool increaseViewCounter, int page, ref long count);
        void AddExternalMedia(string listingId, string appId, IList<MediaFile> media);
        void UpdateExternalMedia(string listingId, string appId, MediaFile media);
        void DeleteExternalMedia(string listingId, string appId, string url);
        void IncreaseCounter(string listingId, string appId, ListingCounters counters, int value);
        void IncreaseCounter(string[] listingId, string appId, ListingCounters counters, int value);
        void Publish(string listingId, string appId);
        void Delete(string listingId, string appId);
        void AddHashtags(string listingId, string appId, string[] hashtags);
        void RemoveHashtags(string listingId, string appId, string[] hashtags);
        string Insert(Listing listing);
        void Update(Listing listing);
    }
}
