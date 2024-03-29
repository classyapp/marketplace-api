﻿using ServiceStack.FluentValidation;

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
                When(x => x.Pricing != null, () =>
                {
                    RuleFor(x => x.Pricing).SetValidator(new PricingInfoValidator())
                        .WithErrorCode("Invalid Pricing Information");
                });
            });
        }
    }
}