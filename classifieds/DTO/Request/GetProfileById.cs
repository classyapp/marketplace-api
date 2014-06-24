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
        public bool IncludeCollections { get; set; }
        public bool IncludeFavorites { get; set; }
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