using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using ServiceStack.Common.Extensions;

namespace classy.Manager.Search
{
    public class ProfileIndexer : IIndexer<Profile>
    {
        private readonly ISearchClientFactory _searchClientFactory;
        private readonly IAppManager _appManager;

        public ProfileIndexer(ISearchClientFactory searchClientFactory, IAppManager appManager)
        {
            _searchClientFactory = searchClientFactory;
            _appManager = appManager;
        }

        public void Index(Profile[] entities, string appId)
        {
            var client = _searchClientFactory.GetClient(ProfileIndexDto.IndexName, appId);
            var indexingInfo = _appManager.GetAppById(appId).IndexingInfo;

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
                        .Where(x => indexingInfo.MetadataPerListing["Profile"].Contains(x.Key))
                        .Select(x => x.Value).ToArray(),
                    Rank = entity.Rank,
                    ReviewAverageScore = entity.ReviewAverageScore,
                    ReviewCount = entity.ReviewCount,
                    ViewCount = entity.ViewCount
                });
            }
            client.IndexMany(profilesToIndex);
        }

        public void RemoveFromIndex(Profile entity, string appId)
        {
            var client = _searchClientFactory.GetClient(ProfileIndexDto.IndexName, appId);
            client.Delete(new ProfileIndexDto {
                Id = entity.Id
            });
        }

        public void Increment<T>(string id, string appId, Expression<Func<Profile, T>> property, int amount = 1)
        {
            var propertyName = ((MemberExpression)property.Body).Member.Name;
            var elasticPropertyName = Char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

            var script = string.Format("ctx._source.{0} += {1}", elasticPropertyName, amount);

            var client = _searchClientFactory.GetClient(ProfileIndexDto.IndexName, appId);
            client.Update<ProfileIndexDto>(d => d
                .Id(id)
                .Script(script));
        }

        public void Increment<T>(string[] ids, string appId, Expression<Func<Profile, T>> property, int amount = 1)
        {
            ids.ForEach(x => Increment(x, appId, property, amount));
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