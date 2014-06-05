using ServiceStack.FluentValidation;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class PostCommentForListing : PostComment
    {
        public string ListingId { get; set; }
    }

    public class PostCommentForListingValidator : AbstractValidator<PostCommentForListing>
    {
        public PostCommentForListingValidator()
        {
            RuleSet(HttpMethods.Post, () =>
            {
                RuleFor(x => x.Content).NotEmpty();
            });
        }
    }
}