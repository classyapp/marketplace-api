using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class DeleteTempMediaRequest  :BaseRequestDto
    {
        public string ListingId { get; set; }
        public string FileId { get; set; }
    }

    public class DeleteTempMediaRequestValidator : AbstractValidator<DeleteTempMediaRequest>
    {
        public DeleteTempMediaRequestValidator()
        {
            RuleFor(x => x.FileId).NotEmpty();
        }
    }
}