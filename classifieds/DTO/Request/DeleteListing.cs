using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class DeleteListing : BaseRequestDto
    {
        public string ListingId { get; set; }
    }

    public class DeleteListingValidator : AbstractValidator<DeleteListing>
    {
        public DeleteListingValidator()
        {
            RuleFor(x => x.ListingId).NotNull();
        }
    }
}