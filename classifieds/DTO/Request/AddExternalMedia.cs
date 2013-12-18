using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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