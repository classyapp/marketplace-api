using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Nest;
using ServiceStack.Common.Extensions;

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

        public void RemoveFromIndex(Profile entity)
        {
            _client.Delete(new ProfileIndexDto {
                Id = entity.Id
            });
        }

        public void Increment<T>(string id, Expression<Func<Profile, T>> property, int amount = 1)
        {
            var propertyName = ((MemberExpression)property.Body).Member.Name;
            var elasticPropertyName = Char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

            var script = string.Format("ctx._source.{0} += {1}", elasticPropertyName, amount);

            _client.Update<ProfileIndexDto>(d => d
                .Id(id)
                .Script(script));
        }

        public void Increment<T>(string[] ids, Expression<Func<Profile, T>> property, int amount = 1)
        {
            ids.ForEach(x => Increment(x, property, amount));
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