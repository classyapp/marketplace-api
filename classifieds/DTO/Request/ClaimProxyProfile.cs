using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class ClaimProxyProfile : BaseRequestDto
    {
        public string ProxyProfileId { get; set; }
        public ProfessionalInfo ProfessionalInfo { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public string DefaultCulture { get; set; }
    }

    public class ClaimProxyProfileValidator : AbstractValidator<ClaimProxyProfile>
    {
        public ClaimProxyProfileValidator()
        {
            RuleFor(x => x.ProfessionalInfo).NotEmpty();
            RuleFor(x => x.DefaultCulture).NotNull();
        }
    }
}