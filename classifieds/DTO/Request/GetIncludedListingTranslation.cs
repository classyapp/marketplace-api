using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class GetIncludedListingTranslation : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public string ListingId { get; set; }
        public string CultureCode { get; set; }
    }
}