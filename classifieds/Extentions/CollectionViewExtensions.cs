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
            to.IncludedListings = from.IncludedListings.Select(l => new Classy.Models.Response.IncludedListingView { Id = l.Id, Comments = l.Comments, ListingType = l.ListingType, ProfileId = from.ProfileId }).ToList();
            if (to.CoverPhotos == null || to.CoverPhotos.Count == 0)
            {
                to.CoverPhotos = null;
            }
            if (from.EditorialFlow != null)
            {
                to.EditorialFlow = new List<EditorialFlowItemView>();
                foreach(var item in from.EditorialFlow)
                {
                    to.EditorialFlow.Add(item.TranslateTo<EditorialFlowItemView>());
                }
            }
            return to;
        }

        public static IList<CollectionView> ToCollectionViewList(this IEnumerable<Collection> from, string culture)
        {
            var to = new List<CollectionView>();
            foreach (var c in from)
            {
                CollectionView v = c.Translate(culture).ToCollectionView();
                to.Add(v);
            }
            return to;
        }
    }
}