﻿using Classy.Models;
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
            to.HasPricingInfo = from.PurchaseOptions != null;
            if (to.HasPricingInfo)
            {
                to.PurchaseOptions = from.PurchaseOptions.ToPurchaseOptionViewList(null);
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

            if (from.TranslatedKeywords != null && from.TranslatedKeywords.Count > 0)
                to.TranslatedKeywords = from.TranslatedKeywords;
            
            return to;
        }

        public static IList<ListingView> ToListingViewList(this IList<Listing> from, string culture)
        {
            var to = new List<ListingView>();
            foreach (var l in from)
            {
                to.Add(l.Translate(culture).ToListingView());
            }
            return to;
        }
    }
}