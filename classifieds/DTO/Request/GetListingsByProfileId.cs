using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class GetListingsByProfileId : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public bool IncludeComments { get; set; }
        public bool FormatCommentsAsHtml { get; set; }
        public bool IncludeDrafts { get; set; }
        public int Page { get; set; }
    }

    public class GetListingsByProfileIdValidator : AbstractValidator<GetListingsByProfileId>
    {
        public GetListingsByProfileIdValidator()
        {
            RuleSet(HttpMethods.Get, () =>
            {
                RuleFor(x => x.ProfileId).NotEmpty();
            });
        }
    }
}