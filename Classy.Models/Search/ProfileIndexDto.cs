﻿using Nest;

namespace Classy.Models.Search
{
    [ElasticType(Name = "profile")]
    public class ProfileIndexDto
    {
        public string Id { get; set; }

        [ElasticProperty(Type = FieldType.geo_point)]
        public GPSLocation Location { get; set; } // from ContatInfo

        [ElasticProperty(Index = FieldIndexOption.not_analyzed)]
        public string Country { get; set; } // country code for fast filtering

        public int Rank { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int ListingCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public int ReviewCount { get; set; }
        public decimal ReviewAverageScore { get; set; }
        
        [ElasticProperty(Index = FieldIndexOption.analyzed)]
        public string[] Metadata { get; set; }
        
        // ProfessionalInfo
        public string CompanyName { get; set; }
        [ElasticProperty(Index = FieldIndexOption.analyzed, OmitNorms = true, SearchAnalyzer = "standard", IndexAnalyzer = "suggest_analyzer")]
        public string AnalyzedCompanyName { get; set; }
        public bool IsVendor { get; set; }
    }
}
