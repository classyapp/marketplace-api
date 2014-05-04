using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class LocalizationResourceViewExtensions
    {
        public static IList<LocalizationResourceView> ToLocalizationResourceViewList(this IList<LocalizationResource> from)
        {
            if (from == null) return null;

            var to = new List<LocalizationResourceView>();
            foreach (var res in from)
            {
                to.Add(res.TranslateTo<LocalizationResourceView>());
            }
            return to;
        }
    }
}