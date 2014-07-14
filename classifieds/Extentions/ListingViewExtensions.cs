using System.Linq;
using Classy.Models;
using Classy.Models.Response;
using System.Collections.Generic;
using ServiceStack.Common;
using Classy.Interfaces.Managers;

namespace classy
{
    public static class ListingTranslateExtensions
    {
        public static ListingView ToListingView(this Listing from, ICurrencyManager currencyManager, string currencyCode)
        {
            var to = from.TranslateTo<ListingView>();
            if (from.ExternalMedia != null) to.ExternalMedia = from.ExternalMedia.ToMediaFileList();
            to.HasPricingInfo = from.PricingInfo != null && from.PricingInfo.BaseOption != null;
            if (to.HasPricingInfo)
            {
                to.PricingInfo = from.PricingInfo.ToPricingInfoView(currencyManager.GetRate(from.PricingInfo.CurrencyCode, currencyCode, (decimal) 0.035));
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
            to.SearchableKeywords = from.SearchableKeywords;
            to.EditorsRank = from.EditorsRank;
            
            return to;
        }

        public static IList<ListingView> ToListingViewList(this IList<Listing> from, string culture, ICurrencyManager currencyManager, string currencyCode)
        {
            var to = new List<ListingView>();
            foreach (var l in from)
            {
                to.Add(l.Translate(culture).ToListingView(currencyManager, currencyCode));
            }
            return to;
        }
    }
}