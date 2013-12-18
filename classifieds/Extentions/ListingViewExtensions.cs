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
            to.HasPricingInfo = from.Pricing != null;
            if (to.HasPricingInfo)
            {
                to.Price = from.Pricing.Price;
                to.CompareAtPrice = from.Pricing.CompareAtPrice;
                to.DomesticRadius = from.Pricing.DomesticRadius;
                to.DomesticShippingPrice = from.Pricing.DomesticShippingPrice;
                to.InternationalShippingPrice = from.Pricing.InternationalShippingPrice;
                to.Quantity = from.Pricing.Quantity;
            }
            to.HasContactInfo = from.ContactInfo != null;
            if (to.HasContactInfo)
            {
                to.ContactInfo = from.ContactInfo.ToExtendedContactInfoView();
            }
            to.HasSchedulingInfo = from.SchedulingTemplate != null;
            if (to.HasSchedulingInfo && from.BookedTimeslots != null)
            {
                to.BookedTimeslots = new List<BookedTimeslotView>();
                foreach (var b in from.BookedTimeslots)
                {
                    to.BookedTimeslots.Add(b.TranslateTo<BookedTimeslotView>());
                }; 
                to.SchedulingTemplate = from.SchedulingTemplate.TranslateTo<TimeslotScheduleView>();
            }
            to.Metadata = from.Metadata.ToCustomAttributeViewList();
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