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
        public bool ForEdit { get; set; }
    }

    public class GetListingByIdValidator : AbstractValidator<GetListingById>
    {
        public GetListingByIdValidator()
        {
            
        }
    }
}