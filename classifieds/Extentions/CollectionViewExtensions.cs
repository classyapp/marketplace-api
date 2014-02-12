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
            to.IncludedListings = from.IncludedListings.Select(l => new Classy.Models.Response.IncludedListingView { ListingId = l.ListingId, Comments = l.Comments }).ToList();
            to.Thumbnails = (from.Thumbnails ?? new List<MediaThumbnail>()).Select(t => new MediaThumbnailView { Height = t.Height, Key = t.Key, Width = t.Width, Url = t.Url }).ToList();
            return to;
        }

        public static IList<CollectionView> ToCollectionViewList(this IEnumerable<Collection> from)
        {
            var to = new List<CollectionView>();
            foreach (var c in from)
            {
                CollectionView v = c.ToCollectionView();
                to.Add(v);
            }
            return to;
        }
    }
}