using System.Collections.Generic;
using Classy.Models.Attributes;

namespace Classy.Models
{
    [MongoCollection(Name = "reviews")]
    public class Review : BaseObject
    {
        public string ProfileId { get; set; }
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
