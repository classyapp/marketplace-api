using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    [Flags]
    public enum ListingUpdateFields
    {
        None = 0,
        Title = 1,
        Content = 2,
        Pricing = 4,
        ContactInfo = 8,
        SchedulingTemplate = 16,
        Metadata = 32
    }

    public class UpdateListing : BaseRequestDto
    {
        public string ListingId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Location Location { get; set; }
        public PricingInfo Pricing { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public TimeslotSchedule SchedulingTemplate { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ListingUpdateFields Fields { get; set; }
    }

    public class UpdateListingValidator : AbstractValidator<UpdateListing>
    {
        public UpdateListingValidator()
        {
            When(x => x.Fields.HasFlag(ListingUpdateFields.Title), () =>
                {
                    RuleFor(x => x.Title).NotEmpty();
                });
            When(x => x.Fields.HasFlag(ListingUpdateFields.ContactInfo), () =>
                {
                    RuleFor(x => x.ContactInfo).Cascade(ServiceStack.FluentValidation.CascadeMode.StopOnFirstFailure).NotNull();
                    RuleFor(x => x.ContactInfo.Location).SetValidator(new LocationValidator());
                });
            When(x => x.Fields.HasFlag(ListingUpdateFields.Pricing), () =>
                {
                    RuleFor(x => x.Pricing).SetValidator(new PricingInfoValidator());
                });
            When(x => x.Fields.HasFlag(ListingUpdateFields.Metadata), () =>
            {
                RuleFor(x => x.Metadata).NotNull();
            });

        }
    }
}