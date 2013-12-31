using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class DeleteExternalMedia : BaseRequestDto
    {
        public string ListingId { get; set; }
        public string Url { get; set; }
    }

    public class DeleteExternalMediaValidator : AbstractValidator<DeleteExternalMedia>
    {
        public DeleteExternalMediaValidator()
        {
            RuleFor(x => x.Url).NotNull();
        }
    }
}