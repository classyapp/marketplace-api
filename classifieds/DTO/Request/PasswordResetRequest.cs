using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class PasswordResetRequest : BaseRequestDto
    {
        public string Hash { get; set; }
        public string Password { get; set; }
    }

    public class PasswordResetRequestValidator : AbstractValidator<PasswordResetRequest>
    {
        public PasswordResetRequestValidator()
        {
            RuleFor(x => x.Hash).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}