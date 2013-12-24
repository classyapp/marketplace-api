using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class PostReviewForProfile : BaseRequestDto
    {
        public string RevieweeProfileId { get; set; }
        public string Content { get; set; }
        public decimal Score { get; set; }
        public IDictionary<string, decimal> SubCriteria { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public IList<CustomAttribute> Metadata { get; set; }
        public bool ReturnReviewerProfile { get; set; }
        public bool ReturnRevieweeProfile { get; set; }
    }

    public class PostReviewForProfileValidator : AbstractValidator<PostReviewForProfile>
    {
        public PostReviewForProfileValidator()
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