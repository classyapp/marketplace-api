using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class SetCollectionCoverPhotos : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public IList<string> Keys { get; set; }
    }

    public class SetCollectionCoverPhotosValidator : AbstractValidator<SetCollectionCoverPhotos>
    {
        public SetCollectionCoverPhotosValidator()
        {
            RuleFor(x => x.CollectionId).NotNull().NotEmpty();
            RuleFor(x => x.Keys).NotNull();
            When(x => x.Keys != null, () =>
            {
                RuleFor(x => x.Keys.Count).GreaterThan(0).LessThanOrEqualTo(4);
            });
        }
    }
}