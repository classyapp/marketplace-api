﻿using System.Collections.Generic;

namespace Classy.Models.Response
{
    public class ReviewView
    {
        public string Id { get; set; }
        public string ProfileId { get; set; }
        public string ReviewerUsername { get; set; }
        public string ReviewerThumbnailUrl { get; set; }
        public string RevieweeProfileId { get; set; }
        public string ListingId { get; set; }
        public string Content { get; set; }
        public decimal Score { get; set; }
        public IDictionary<string, decimal> SubCriteria { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }
    }
}
