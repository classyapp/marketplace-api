using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class SetProfileTranslation : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public string Culture { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class SetProfileTranslationValidator : AbstractValidator<SetProfileTranslation>
    {
        public SetProfileTranslationValidator()
        {
            //RuleFor(x => x.ProfileId).NotNull().NotEmpty();
            //RuleFor(x => x.Culture).NotNull().NotEmpty();

            //RuleFor(x => x.Metadata).NotNull();
        }
    }
}