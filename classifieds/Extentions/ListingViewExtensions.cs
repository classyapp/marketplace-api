using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class ListingTranslateExtensions
    {
        public static ListingView ToListingView(this Listing from)
        {
            var to = from.TranslateTo<ListingView>();
            if (from.ExternalMedia != null) to.ExternalMedia = from.ExternalMedia.ToMediaFileList();
            to.HasPricingInfo = from.PricingInfo != null;
            if (to.HasPricingInfo)
            {
                to.PricingInfo = from.PricingInfo.ToPricingInfoView();
            }
            to.HasContactInfo = from.ContactInfo != null;
            if (to.HasContactInfo)
            {
                to.ContactInfo = from.ContactInfo.ToExtendedContactInfoView();
            }
            to.HasSchedulingInfo = from.SchedulingTemplate != null;
            if (to.HasSchedulingInfo)
            {   
                to.SchedulingTemplate = from.SchedulingTemplate.TranslateTo<TimeslotScheduleView>();
            }
            return to;
        }

        public static IList<ListingView> ToListingViewList(this IList<Listing> from)
        {
            var to = new List<ListingView>();
            foreach (var l in from)
            {
                to.Add(l.ToListingView());
            }
            return to;
        }
    }
}