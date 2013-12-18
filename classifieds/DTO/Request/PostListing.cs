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
        public IList<CustomAttribute> CustomAttributes { get; set; }
    }

    public class PostListingValidator : AbstractValidator<PostListing>
    {
        public PostListingValidator()
        {
            RuleFor(x => x.ListingId).NotEmpty();
            RuleFor(x => x.AppId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
            RuleFor(x => x.ListingType).NotEmpty();
            RuleFor(x => x.ContactInfo).NotNull();
            RuleFor(x => x.ContactInfo.Location).SetValidator(new LocationValidator())
                .WithErrorCode("Invalid Location");
            RuleFor(x => x.Pricing).SetValidator(new PricingInfoValidator())
                .WithErrorCode("Invalid Pricing Information");
        }
    }
}