using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class ReviewViewExtentions
    {
        public static IList<ReviewView> ToReviewViewList(this IEnumerable<Review> from)
        {
            var to = new List<ReviewView>();
            foreach (var c in from)
            {
                to.Add(c.TranslateTo<ReviewView>());
            }
            return to;
        }
    }
}