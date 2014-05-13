﻿using System.Security.Policy;
using Nest;

namespace Classy.Models.Search
{
    [ElasticType(Name = "listing")]
    public class ListingIndexDto
    {
        public string Id { get; set; }

        [ElasticProperty(Index = FieldIndexOption.analyzed)]
        public string Title { get; set; }
        [ElasticProperty(Index = FieldIndexOption.analyzed)]
        public string Content { get; set; }
        [ElasticProperty(Index = FieldIndexOption.not_analyzed)]
        public string ListingType { get; set; }
        [ElasticProperty(Index = FieldIndexOption.analyzed)]
        public string[] Keywords { get; set; } // comes from SearchableKeywords

        // These fields are here for scoring/relevance
        public int CommentCount { get; set; }
        public int FavoriteCount { get; set; }
        public int FlagCount { get; set; }
        public int ViewCount { get; set; }
        public int ClickCount { get; set; }
        public int PurchaseCount { get; set; }
        public int BookingCount { get; set; }
        public int AddToCollectionCount { get; set; }

        // Just added this so we won't have to go to the
        // db just for displaying direct search results
        public string ImageUrl { get; set; }

        // not sure if we need these...
        //public int DisplayOrder { get; set; }
        //public ContactInfo ContactInfo { get; set; }
        //public PricingInfo PricingInfo { get; set; }
        //public TimeslotSchedule SchedulingTemplate { get; set; }

        [ElasticProperty(Index = FieldIndexOption.analyzed)] // should we analyze this data ?...
        public string[] Metadata { get; set; }
    }
}