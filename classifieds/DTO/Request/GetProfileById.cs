using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class GetProfileById : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public bool IncludeFollowedByProfiles { get; set; }
        public bool IncludeFollowingProfiles { get; set; }
        public bool IncludeReviews { get; set; }
        public bool IncludeListings { get; set; }
        public bool LogImpression { get; set; }
    }

    public class GetProfileByIdValidator : AbstractValidator<GetProfileById>
    {
        public GetProfileByIdValidator()
        {
            RuleSet(HttpMethods.Get, () =>
            {
                RuleFor(x => x.ProfileId).NotEmpty();
            });
        }
    }
}