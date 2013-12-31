using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class BookListing : BaseRequestDto
    {
        public BookListing()
        {
            MaxDoubleBookings = int.MaxValue;
        }

        public string ListingId { get; set; }
        public DateRange DateRange { get; set; }
        public int MaxDoubleBookings { get; set; }
        public string Comment { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class BookListingValidator : AbstractValidator<BookListing>
    {
        public BookListingValidator()
        {
            RuleFor(x => x.DateRange).NotNull();
            RuleFor(x => x.PaymentMethod)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .SetValidator(new PaymentMethodValidator());
        }
    }
}