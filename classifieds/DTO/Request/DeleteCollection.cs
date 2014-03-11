using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class DeleteCollection : BaseRequestDto
    {
        public string CollectionId { get; set; }
    }

    public class DeleteCollectionValidator : AbstractValidator<DeleteCollection>
    {
        public DeleteCollectionValidator()
        {
            RuleFor(x => x.CollectionId).NotNull();
        }
    }
}