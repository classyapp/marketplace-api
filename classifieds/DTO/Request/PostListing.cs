using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class PostListing : BaseRequestDto
    {
        public string ListingId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ListingType { get; set; }
        public Location Location { get; set; }
        public PricingInfo Pricing { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public TimeslotSchedule SchedulingTemplate { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class PostListingValidator : AbstractValidator<PostListing>
    {
        public PostListingValidator()
        {
            When(x => string.IsNullOrEmpty(x.ListingId), () =>
            {
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.ListingType).NotEmpty();
                When(x => x.ContactInfo != null, () =>
                {
                    RuleFor(x => x.ContactInfo.Location).SetValidator(new LocationValidator())
                        .WithErrorCode("Invalid Location");
                });
                When(x => x.Pricing != null, () =>
                {
                    RuleFor(x => x.Pricing).SetValidator(new PricingInfoValidator())
                        .WithErrorCode("Invalid Pricing Information");
                });
            });
        }
    }
}