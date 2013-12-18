using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class GetOrdersForListing : BaseRequestDto
    {
        public string ListingId { get; set; }
        public bool IncludeCancelled { get; set; }
    }
}