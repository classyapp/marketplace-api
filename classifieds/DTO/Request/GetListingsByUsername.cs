using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class GetListingsByUsername : BaseRequestDto
    {
        public string Username { get; set; }
        public bool IncludeComments { get; set; }
        public bool FormatCommentsAsHtml { get; set; }
    }

    public class GetListingsByUsernameValidator : AbstractValidator<GetListingsByUsername>
    {
        public GetListingsByUsernameValidator()
        {
            RuleSet(HttpMethods.Get, () =>
            {
                RuleFor(x => x.Username).NotEmpty();
            });
        }
    }
}