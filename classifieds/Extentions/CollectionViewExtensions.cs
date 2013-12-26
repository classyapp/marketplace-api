using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class CollectionViewExtentions
    {
        public static IList<CollectionView> ToCollectionViewList(this IEnumerable<Collection> from)
        {
            var to = new List<CollectionView>();
            foreach (var c in from)
            {
                to.Add(c.TranslateTo<CollectionView>());
            }
            return to;
        }
    }
}