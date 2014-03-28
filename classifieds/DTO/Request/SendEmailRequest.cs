using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class SendEmailRequest : BaseRequestDto
    {
        public string ReplyTo { get; set; }
        public string[] To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Template { get; set; }
        public Dictionary<string, string> Variables { get; set; }
    }

    public class SendEmailRequestValidator : AbstractValidator<SendEmailRequest>
    {
        public SendEmailRequestValidator()
        {
            RuleFor(x => x.To).NotEmpty();
            RuleFor(x => x.Subject).NotEmpty();
        }
    }
}