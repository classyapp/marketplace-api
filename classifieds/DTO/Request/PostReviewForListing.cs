using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class PostReviewForListing : BaseRequestDto
    {
        public string ListingId { get; set; }
        public string Content { get; set; }
        public int Score { get; set; }
    }

    public class PostReviewForListingValidator : AbstractValidator<PostReviewForListing>
    {
        public PostReviewForListingValidator()
        {
            RuleSet(HttpMethods.Post, () =>
            {
                RuleFor(x => x.Content).NotEmpty();
                RuleFor(x => x.Score)
                    .GreaterThanOrEqualTo(1)
                    .LessThanOrEqualTo(5)
                    .WithMessage("the scrore must be in the range 1-5");
            });
        }
    }
}