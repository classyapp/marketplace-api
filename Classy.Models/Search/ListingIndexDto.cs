using System.Collections.Generic;
using Nest;

namespace Classy.Models.Search
{
    [ElasticType(Name = "listing")]
    public class ListingIndexDto
    {
        [ElasticProperty(Index = FieldIndexOption.analyzed)]
        public string Title { get; set; }
        [ElasticProperty(Index = FieldIndexOption.analyzed)]
        public string Content { get; set; }
        public string ListingType { get; set; }
        public string[] Keywords { get; set; } // comes from SearchableKeywords

        // Scoring
        public int CommentCount { get; set; }
        public int FavoriteCount { get; set; }
        public int FlagCount { get; set; }
        public int ViewCount { get; set; }
        public int ClickCount { get; set; }
        public int PurchaseCount { get; set; }
        public int BookingCount { get; set; }
        public int AddToCollectionCount { get; set; }

        // not sure if we need these...
        //public int DisplayOrder { get; set; }
        //public ContactInfo ContactInfo { get; set; }
        //public PricingInfo PricingInfo { get; set; }
        //public TimeslotSchedule SchedulingTemplate { get; set; }

        [ElasticProperty(Type = FieldType.@object)]
        public Dictionary<string, string> Metadata { get; set; }
    }
}