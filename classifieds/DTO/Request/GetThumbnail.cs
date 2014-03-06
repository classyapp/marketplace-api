using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class GetThumbnail : BaseRequestDto
    {
        public string ImageKey { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class GetThumbnailValidator : AbstractValidator<GetThumbnail>
    {
        public GetThumbnailValidator()
        {
            RuleFor(x => x.ImageKey).NotEmpty();
            RuleFor(x => x.Width).GreaterThan(0);
        }
    }
}