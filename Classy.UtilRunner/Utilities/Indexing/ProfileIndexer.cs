using System;
using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Funq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

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
            BsonClassMap.RegisterClassMap<Profile>(c => c.AutoMap());
            _profiles = mongoDatabase.GetCollection<Profile>("profiles");
        }
        
        public StatusCode Run(string[] args)
        {
            var indexName = "profiles" + "_v1.0";

            //var client = _searchClientFactory.GetClient("profiles", "v1.0");
            //client.DeleteIndex(d => d.Index<ProfileIndexDto>());
            //client.CreateIndex("profiles_v1.0", s => s.Settings(_ => new IndexSettings()));
            //client.Map<ProfileIndexDto>(
            //    m => m.MapFromAttributes().Properties(
            //        p => p.Completion(
            //            c => c.Name(n => n.CompanyName).IndexAnalyzer("simple").SearchAnalyzer("simple")
            //                .Payloads(true).MaxInputLength(20))));

            var client = _searchClientFactory.GetClient("profiles", "v1.0");
            client.Map<ProfileIndexDto>(m => m.MapFromAttributes());

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
                        CompanyName = professional.ProfessionalInfo.CompanyName,
                        AnalyzedCompanyName = professional.ProfessionalInfo.CompanyName,
                        FollowerCount= professional.FollowerCount,
                        FollowingCount= professional.FollowingCount,
                        IsVendor= professional.IsVendor,
                        ListingCount= professional.ListingCount,
                        Location= GetGpsLocation(professional),
                        Country = GetCountry(professional),
                        Metadata = professional.Metadata.Select(x => x.Value).ToArray(),
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

        private string GetCountry(Profile professional)
        {
            if (professional.ProfessionalInfo == null)
                return null;
            if (professional.ProfessionalInfo.CompanyContactInfo == null)
                return null;
            if (professional.ProfessionalInfo.CompanyContactInfo.Location == null)
                return null;
            if (professional.ProfessionalInfo.CompanyContactInfo.Location.Address == null)
                return null;
            return professional.ProfessionalInfo.CompanyContactInfo.Location.Address.Country;
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