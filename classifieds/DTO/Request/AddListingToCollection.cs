using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class AddListingsToCollection : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public string[] IncludedListings { get; set; }
    }

    public class AddListingsToCollectionValidator : AbstractValidator<AddListingsToCollection>
    {
        public AddListingsToCollectionValidator()
        {
            RuleFor(x => x.IncludedListings).NotNull();
        }
    }
}