using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class JobsStatusRequest : BaseRequestDto
    {
        public string ProfileID { get; set; }
    }

    public class JobsStatusRequestValidator : AbstractValidator<JobsStatusRequest>
    {
        public JobsStatusRequestValidator()
        {
            RuleFor(x => x.ProfileID).NotEmpty();
        }
    }
}