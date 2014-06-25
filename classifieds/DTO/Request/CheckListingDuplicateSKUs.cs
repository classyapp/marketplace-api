using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class CheckListingDuplicateSKUs : BaseRequestDto
    {
        public string[] SKUs { get; set; }
        public string ListingId { get; set; }
    }

    public class CheckListingDuplicateSKUsValidator : AbstractValidator<CheckListingDuplicateSKUs>
    {
        public CheckListingDuplicateSKUsValidator()
        {
            RuleFor(x => x.SKUs).NotNull();
        }
    }
}