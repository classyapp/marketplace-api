using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class GetCitiesByCountry : BaseRequestDto
    {
        public string CountryCode { get; set; }
    }

    public class GetCitiesByCountryValidator : AbstractValidator<GetCitiesByCountry>
    {
        public GetCitiesByCountryValidator()
        {
            RuleFor(x => x.CountryCode)
                .NotNull()
                .NotEmpty()
                .Length(2);
        }
    }
}