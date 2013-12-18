using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class PurchaseSingleItem : BaseRequestDto
    {
        public string ListingId { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PhysicalAddress ShippingAddress { get; set; }
    }

    public class PurchaseSingleItemValidator : AbstractValidator<PurchaseSingleItem>
    {
        public PurchaseSingleItemValidator()
        {
            RuleFor(x => x.ListingId).NotEmpty();
            RuleFor(x => x.AppId).NotEmpty();
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("'quantity' larger than 0 is required");
            RuleFor(x => x.PaymentMethod)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .SetValidator(new PaymentMethodValidator());
        }
    }
}