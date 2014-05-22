using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class PricingInfoViewExtentions
    {
        public static PricingInfoView ToPricingInfoView(this PricingInfo from, double adjustRate)
        {
            var to = from.TranslateTo<PricingInfoView>();
            if (from.PurchaseOptions != null)
            {
                if (to.PurchaseOptions == null) to.PurchaseOptions = new List<PurchaseOptionView>();
                foreach (var o in from.PurchaseOptions)
                {
                    var v = o.TranslateTo<PurchaseOptionView>();
                    v.Price *= adjustRate;
                    if (v.CompareAtPrice.HasValue) { v.CompareAtPrice *= adjustRate; }
                    v.MediaFiles = o.MediaFiles.Select(m => m.TranslateTo<MediaFileView>()).ToArray();
                    to.PurchaseOptions.Add(v);
                }
            }
            return to;
        }
    }
}