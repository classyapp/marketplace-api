using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class PostCommentForCollection : PostComment
    {
        public string CollectionId { get; set; }
    }

    public class PostCommentForCollectionValidator : AbstractValidator<PostCommentForCollection>
    {
        public PostCommentForCollectionValidator()
        {
            RuleSet(HttpMethods.Post, () =>
            {
                RuleFor(x => x.Content).NotEmpty();
            });
        }
    }
}