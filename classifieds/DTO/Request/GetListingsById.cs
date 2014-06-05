using Classy.Models;
using ServiceStack.FluentValidation;

namespace classy.DTO.Request
{
    public class GetListingsById : BaseRequestDto
    {
        public string[] ListingIds { get; set; }
    }

    public class GetListingsByIdValidator : AbstractValidator<GetListingsById>
    {
        public GetListingsByIdValidator()
        {
        }
    }
}