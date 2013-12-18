using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class GetListingById : BaseRequestDto
    {
        public string ListingId { get; set; }
        public bool IncludeComments { get; set; }
        public bool FormatCommentsAsHtml { get; set; }
        public bool IncludeProfile { get; set; }
        public bool IncludeCommenterProfiles { get; set; }
        public bool IncludeFavoritedByProfiles { get; set; }
        public bool LogImpression { get; set; }
    }

    public class GetListingByIdValidator : AbstractValidator<GetListingById>
    {
        public GetListingByIdValidator()
        {
            
        }
    }
}