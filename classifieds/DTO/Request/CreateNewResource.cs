using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class CreateNewResource : BaseRequestDto
    {
        public string Key { get; set; }
        public IDictionary<string, string> Values { get; set; }
        public string Description { get; set; }
    }

    public class CreateNewResourceValidator : AbstractValidator<CreateNewResource>
    {
        public CreateNewResourceValidator() 
        {
            RuleFor(x => x.Key).NotNull();
            RuleFor(x => x.Description).NotNull();
        }
    }
}