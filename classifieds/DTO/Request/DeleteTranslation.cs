using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class DeleteTranslation : BaseRequestDto
    {
        public string CultureCode { get; set; }
    }

    public class DeleteTranslationValidator : AbstractValidator<DeleteTranslation>
    {
        public DeleteTranslationValidator()
        {
            RuleFor(x => x.CultureCode).NotNull().NotEmpty();
        }
    }

    public class DeleteProfileTranslation : DeleteTranslation
    {
        public string ProfileId { get; set; }
    }

    public class DeleteListingTranslation : DeleteTranslation
    {
        public string ListingId { get; set; }
    }

    public class DeleteCollectionTranslation : DeleteTranslation
    {
        public string CollectionId { get; set; }
    }
}