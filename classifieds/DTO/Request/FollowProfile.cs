using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class FollowProfile : BaseRequestDto
    {
        public string FolloweeUsername { get; set; }
    }

    public class FollowProfileValidator : AbstractValidator<FollowProfile>
    {
        public FollowProfileValidator()
        {
            RuleFor(x => x.AppId).NotEmpty();
            RuleFor(x => x.FolloweeUsername).NotEmpty();
        }
    }
}