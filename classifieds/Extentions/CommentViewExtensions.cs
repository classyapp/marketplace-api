using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class CommentViewExtentions
    {
        public static IList<CommentView> ToCommentViewList(this IEnumerable<Comment> from)
        {
            var to = new List<CommentView>();
            foreach (var c in from)
            {
                to.Add(c.TranslateTo<CommentView>());
            }
            return to;
        }
    }
}