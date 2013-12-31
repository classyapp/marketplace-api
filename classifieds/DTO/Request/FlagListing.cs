using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class FlagListing : BaseRequestDto
    {
        public string ListingId { get; set; }
        public FlagReason FlagReason { get; set; }
    }

    public class FlagListingValidator : AbstractValidator<FlagListing>
    {
        public FlagListingValidator()
        {
            RuleFor(x => x.FlagReason).NotEqual(FlagReason.Invalid);
        }
    }
}