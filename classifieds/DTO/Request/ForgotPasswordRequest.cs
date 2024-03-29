﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class ForgotPasswordRequest : BaseRequestDto
    {
        public string Host { get; set; }
        public string Email { get; set; }
    }

    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Host).NotEmpty();
            RuleFor(x => x.Email).NotEmpty();
        }
    }
}