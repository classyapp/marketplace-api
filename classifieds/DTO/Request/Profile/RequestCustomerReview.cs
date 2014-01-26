using Classy.Models;
using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.DTO.Request.Profile
{
    public class RequestCustomerReview : BaseRequestDto
    {
        public string ReviewUrl { get; set; }
        public string ProfileId { get; set; }
        public IList<string> Emails { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string ReplyToEmail { get; set; }
    }

    public class AddExternalMediaValidator : AbstractValidator<RequestCustomerReview>
    {
        public AddExternalMediaValidator()
        {
            RuleFor(x => x.Emails).NotEmpty(); // any way to plug into service stack to check that each string is an email? there's a Must() extension but no email address validation afaict
            RuleFor(x => x.Subject).NotEmpty();
        }
    }
}