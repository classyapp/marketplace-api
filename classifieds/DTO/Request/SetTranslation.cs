using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class SetTranslation : BaseRequestDto
    {
        public string CultureCode { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class SetTranslationValidator : AbstractValidator<SetTranslation>
    {
        public SetTranslationValidator()
        {
            RuleFor(x => x.CultureCode).NotNull().NotEmpty();
        }
    }

    public class SetProfileTranslation : SetTranslation
    {
        public string ProfileId { get; set; }
        public string CompanyName { get; set; }
    }

    public class SetListingTranslation : SetTranslation
    {
        public string ListingId { get; set; } 
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class SetCollectionTranslation : SetTranslation
    {
        public string CollectionId { get; set; } 
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class SetIncludedListingTranslation : SetTranslation
    {
        public string CollectionId { get; set; }
        public string ListingId { get; set; }
        public string Comment { get; set; }
    }
}