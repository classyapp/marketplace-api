using classy.Extentions;
using Classy.Models;
using Classy.Models.Response;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Common;

namespace classy
{
    public static class PricingInfoViewExtentions
    {
        public static PricingInfoView ToPricingInfoView(this PricingInfo from, decimal adjustRate)
        {
            var to = from.TranslateTo<PricingInfoView>();
            to.BaseOption = from.BaseOption.TranslateTo<PurchaseOptionView>();
            if (to.BaseOption.CompareAtPrice.HasValue) to.BaseOption.CompareAtPrice = to.BaseOption.CompareAtPrice *= adjustRate;
            to.BaseOption.Price *= adjustRate;
<<<<<<< HEAD
            to.BaseOption.MediaFiles = from.BaseOption.MediaFiles.EmptyIfNull().Select(m => m.TranslateTo<MediaFileView>()).ToArray();
=======
            to.BaseOption.MediaFiles = from.BaseOption.MediaFiles.Select(m => m.TranslateTo<MediaFileView>()).ToArray();
>>>>>>> origin/master
            if (from.PurchaseOptions != null)
            {
                if (to.PurchaseOptions == null) to.PurchaseOptions = new List<PurchaseOptionView>();
                foreach (var o in from.PurchaseOptions)
                {
                    var v = o.TranslateTo<PurchaseOptionView>();
                    v.Price *= adjustRate;
                    if (v.CompareAtPrice.HasValue) { v.CompareAtPrice *= adjustRate; }
                    if (o.MediaFiles != null)
                    {
                        v.MediaFiles = o.MediaFiles.Select(m => m.TranslateTo<MediaFileView>()).ToArray();
                    }
                    to.PurchaseOptions.Add(v);
                }
            }
            return to;
        }
    }
}