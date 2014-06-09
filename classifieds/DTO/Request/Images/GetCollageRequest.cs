using System.Data;
using Classy.Models;
using ServiceStack.FluentValidation;

namespace classy.DTO.Request.Images
{
    public class GetCollageRequest : BaseRequestDto
    {
        public string[] ImageKeys { get; set; }
    }

    public class GetCollageRequestValidator : AbstractValidator<GetCollageRequest>
    {
        public GetCollageRequestValidator()
        {
            RuleFor(x => x.ImageKeys).NotEmpty();
        }
    }
}