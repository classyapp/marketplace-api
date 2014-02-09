using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class CreateProfileProxy : BaseRequestDto
    {
        public string BatchId { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ProfessionalInfo ProfessionalInfo { get; set; }
    }

    public class CreateProfileProxyValidator : AbstractValidator<CreateProfileProxy>
    {
        public CreateProfileProxyValidator() {
            RuleFor(x => x.ProfessionalInfo).NotNull();
        }
    }
}