using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class GetListingMoreInfo : BaseRequestDto
    {
        public string ListingId { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class GetListingMoreInfoValidator : AbstractValidator<GetListingMoreInfo>
    {
        public GetListingMoreInfoValidator()
        {
            RuleFor(x => x.ListingId).NotEmpty();
        }
    }
}