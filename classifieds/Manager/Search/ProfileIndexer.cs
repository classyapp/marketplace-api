using System.Collections.Generic;
using System.Linq;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public class ProfileIndexer : IIndexer<Profile>
    {
        private readonly ElasticClient _client;
        private readonly IndexingInfo _indexingInfo;

        public ProfileIndexer(ISearchClientFactory searchClientFactory, IAppManager appManager)
        {
            _indexingInfo = appManager.GetIndexingInfo();
            _client = searchClientFactory.GetClient("profiles");
        }

        public void Index(Profile[] entities)
        {
            var profilesToIndex = new List<ProfileIndexDto>();
            foreach (var entity in entities)
            {
                if (!entity.IsProfessional)
                    continue;

                profilesToIndex.Add(new ProfileIndexDto {
                    CommentCount = entity.CommentCount,
                    ComnpanyName = entity.ProfessionalInfo.CompanyName,
                    FollowerCount = entity.FollowerCount,
                    FollowingCount = entity.FollowingCount,
                    IsVendor = entity.IsVendor,
                    ListingCount = entity.ListingCount,
                    Location = GetGpsLocation(entity),
                    Metadata = entity.Metadata
                        .Where(x => _indexingInfo.MetadataPerListing["Profile"].Contains(x.Key))
                        .Select(x => x.Value).ToArray(),
                    Rank = entity.Rank,
                    ReviewAverageScore = entity.ReviewAverageScore,
                    ReviewCount = entity.ReviewCount,
                    ViewCount = entity.ViewCount
                });
            }
            _client.IndexMany(profilesToIndex);
        }

        private static GPSLocation GetGpsLocation(Profile professional)
        {
            if (professional.ContactInfo == null)
                return null;
            if (professional.ContactInfo.Location == null)
                return null;
            if (professional.ContactInfo.Location.Coords == null)
                return null;

            var coords = professional.ContactInfo.Location.Coords;
            if (!coords.Longitude.HasValue || !coords.Latitude.HasValue)
                return null;

            return new GPSLocation
            {
                Longitude = coords.Longitude.Value,
                Latitude = coords.Latitude.Value
            };
        }
    }
}