using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class RemoveListingFromCollection : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public string[] ListingIds { get; set; }
    }

    public class RemoveListingFromCollectionValidator : AbstractValidator<RemoveListingFromCollection>
    {
        public RemoveListingFromCollectionValidator()
        {
            RuleFor(x => x.CollectionId).NotNull();
            RuleFor(x => x.ListingIds).NotNull();
        }
    }
}