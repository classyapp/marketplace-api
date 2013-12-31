using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class Review : BaseObject
    {
        public string ProfileId { get; set; }
        public string RevieweeProfileId { get; set; }
        public string ListingId { get; set; }
        public string Content { get; set; }
        public decimal Score { get; set; }
        public IDictionary<string, decimal> SubCriteria { get; set; }
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }
    }
}
