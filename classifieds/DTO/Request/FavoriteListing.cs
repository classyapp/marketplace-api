using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class FavoriteListing : BaseRequestDto
    {
        public string ListingId { get; set; }
    }

    public class FavoriteListingValidator : AbstractValidator<FavoriteListing>
    {
        public FavoriteListingValidator()
        {
            RuleFor(x => x.ListingId).NotEmpty();
        }
    }
}