using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public enum ObjectType
    {
        Listing,
        Collection
    }

    public class PostComment : BaseRequestDto
    {
        public string ObjectId { get; set; }
        public string Content { get; set; }
        public bool FormatAsHtml { get; set; }
        public ObjectType Type { get; set; }
    }

    public class PostCommentValidator : AbstractValidator<PostComment>
    {
        public PostCommentValidator()
        {
            RuleSet(HttpMethods.Post, () =>
            {
                RuleFor(x => x.Content).NotEmpty();
            });
        }
    }
}