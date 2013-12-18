using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models
{
    public class PaymentMethod
    {
        public string Name { get; set; }
        public string Provider { get; set; }
        public IList<CustomAttribute> Data { get; set; }
    }

    public class PaymentMethodValidator : AbstractValidator<PaymentMethod>
    {
        public PaymentMethodValidator()
        {
            RuleFor(x => x.Name).Must((name) =>
            {
                return name == "card";
            }).WithMessage("Only 'card' is allowed as PaymentMethod");
            RuleFor(x => x.Data)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .Must((data) =>
                {
                    return data.ContainsKey("address_line_1");
                })
                .WithMessage("The 'data' field is the required 'address_line_1' field");
                //.Must((data) =>
                //{
                //    return data.ContainsKey("address_line_1");
                //})
                //.WithMessage("The 'data' field is the required 'address_line_1' field")
                //.Must((data) =>
                //{
                //    return data.ContainsKey("address_line_1");
                //})
                //.WithMessage("The 'data' field is the required 'address_line_1' field");
        }
    }
}