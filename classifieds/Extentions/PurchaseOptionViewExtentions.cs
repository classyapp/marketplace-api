using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using Classy.Models.Response;
using ServiceStack.Common;

namespace classy
{
    public static class PurchaseOptionViewExtentions
    {
        public static PurchaseOptionView ToPurchaseOptionView(this PurchaseOption from)
        {
            var to = from.TranslateTo<PurchaseOptionView>();
            to.MediaFiles = from.MediaFiles.ToMediaFileList();
            return to;
        }

        public static IList<PurchaseOptionView> ToPurchaseOptionViewList(this IList<PurchaseOption> from, string culture)
        {
            var to = new List<PurchaseOptionView>();
            foreach (var po in from)
            {
                PurchaseOptionView v = po.Translate(culture).ToPurchaseOptionView();
                to.Add(v);
            }
            return to;
        }
    }
}