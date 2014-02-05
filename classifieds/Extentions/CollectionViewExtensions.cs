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
        public static CollectionView ToCollectionView(this Collection from)
        {
            var to = from.TranslateTo<CollectionView>();
            to.IncludedListings = from.IncludedListings.Select(l => new Classy.Models.Response.IncludedListing { ListingId = l.ListingId, Comments = l.Comments }).ToList();
            return to;
        }

        public static IList<CollectionView> ToCollectionViewList(this IEnumerable<Collection> from)
        {
            var to = new List<CollectionView>();
            foreach (var c in from)
            {
                CollectionView v = c.TranslateTo<CollectionView>();
                if (c.IncludedListings == null)
                    v.IncludedListings = new Classy.Models.Response.IncludedListing[0];
                else
                    v.IncludedListings = c.IncludedListings.Select(l => new Classy.Models.Response.IncludedListing { ListingId = l.ListingId, Comments = l.Comments }).ToArray();
                to.Add(v);
            }
            return to;
        }
    }
}