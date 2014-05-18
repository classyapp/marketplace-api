using System;
using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Funq;
using MongoDB.Driver;
using Nest;

namespace Classy.UtilRunner.Utilities.Indexing
{
    public class ProfileIndexer : IUtility
    {
        private const int BatchSize = 200;
        private readonly MongoCollection<Profile> _profiles;
        private readonly ISearchClientFactory _searchClientFactory;

        public ProfileIndexer(Container container)
        {
            _searchClientFactory = container.Resolve<ISearchClientFactory>();
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _profiles = mongoDatabase.GetCollection<Profile>("profiles");
        }
        
        public StatusCode Run(string[] args)
        {
            var indexName = "profiles" + "_v1.0";

            var client = _searchClientFactory.GetClient("profiles", "v1.0");
            client.DeleteIndex<ProfileIndexDto>();
            client.CreateIndex("profiles_v1.0", new IndexSettings());
            client = _searchClientFactory.GetClient("profiles", "v1.0");
            client.MapFromAttributes<ProfileIndexDto>();

            client = _searchClientFactory.GetClient("profiles", "v1.0");

            var i = 0;
            var cursor = _profiles.Find(MongoDB.Driver.Builders.Query<Profile>.NE(x => x.ProfessionalInfo, null)).SetBatchSize(BatchSize);
            foreach (var professionalsBulks in cursor.Bulks(BatchSize))
            {
                var toIndex = new List<ProfileIndexDto>(BatchSize);

                foreach (var professional in professionalsBulks)
                {
                    toIndex.Add(new ProfileIndexDto {
                        Id = professional.Id,
                        CommentCount = professional.CommentCount,
                        ComnpanyName = professional.ProfessionalInfo.CompanyName,
                        FollowerCount= professional.FollowerCount,
                        FollowingCount= professional.FollowingCount,
                        IsVendor= professional.IsVendor,
                        ListingCount= professional.ListingCount,
                        Location= GetGpsLocation(professional),
                        Metadata = professional.Metadata.Select(x => x.Value).ToArray(),
                        Rank= professional.Rank,
                        ReviewAverageScore= professional.ReviewAverageScore,
                        ReviewCount= professional.ReviewCount,
                        ViewCount= professional.ViewCount
                    });
                }

                client.IndexMany(toIndex);
                
                Console.WriteLine("Indexed {0} documents", ++i*BatchSize);
            }

            return StatusCode.Success;
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

            return new GPSLocation {
                Longitude = coords.Longitude.Value,
                Latitude = coords.Latitude.Value
            };
        }
    }
}