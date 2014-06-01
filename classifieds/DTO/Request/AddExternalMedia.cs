using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class AddExternalMedia : BaseRequestDto
    {
        public string ListingId { get; set; }
    }

    public class AddExternalMediaValidator : AbstractValidator<AddExternalMedia>
    {
        public AddExternalMediaValidator()
        {
            RuleFor(x => x.ListingId).NotEmpty();
        }
    }
}