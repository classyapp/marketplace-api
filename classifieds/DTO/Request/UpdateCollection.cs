using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;

namespace Classy.Models.Request
{
    public class UpdateCollection : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string EditorialEditorialApprovalBy { get; set; }
        public IList<IncludedListing> IncludedListings { get; set; }
    }
}