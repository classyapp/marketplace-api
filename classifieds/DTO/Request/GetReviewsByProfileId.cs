using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class GetReviewsByProfileId : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public bool IncludeDrafts { get; set; }
        public bool IncludeOnlyDrafts { get; set; }
    }

    public class GetReviewsByProfileIdValidator : AbstractValidator<GetReviewsByProfileId>
    {
        public GetReviewsByProfileIdValidator()
        {
            RuleFor(x => x.IncludeOnlyDrafts)
                .Must(VerifyDraftsStatus)
                .WithMessage("when IncludeOnlyDrafts is true, IncludeDrafts must also be true");
        }

        private bool VerifyDraftsStatus(GetReviewsByProfileId request, bool includeOnlyDrafts)
        {
            return includeOnlyDrafts ?
                request.IncludeDrafts : true;
        }
    }
}