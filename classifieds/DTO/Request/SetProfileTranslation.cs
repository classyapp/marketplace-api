using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class SetTranslation : BaseRequestDto
    {
        public string ObjectId { get; set; }
        public string Culture { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class SetTranslationValidator : AbstractValidator<SetTranslation>
    {
        public SetTranslationValidator()
        {
            RuleFor(x => x.ObjectId).NotNull().NotEmpty();
            RuleFor(x => x.Culture).NotNull().NotEmpty();
        }
    }
}