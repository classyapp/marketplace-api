using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class EditListing : PostListing
    {
    }

    public class EditListingValidator : PostListingValidator
    {
        public EditListingValidator()
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
                When(x => x.PurchaseOptions != null, () =>
                {
                    RuleFor(x => x.PurchaseOptions.Count).GreaterThan(0);
                });
            });
        }
    }
}