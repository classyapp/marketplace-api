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
        public static PricingInfoView ToPricingInfoView(this PricingInfo from)
        {
            var to = from.TranslateTo<PricingInfoView>();
            if (from.PurchaseOptions != null)
            {
                if (to.PurchaseOptions == null) to.PurchaseOptions = new List<PurchaseOptionView>();
                foreach (var o in from.PurchaseOptions)
                {
                    to.PurchaseOptions.Add(o.TranslateTo<PurchaseOptionView>());
                }
            }
            return to;
        }
    }
}