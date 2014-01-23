using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class LocalizationListResourceViewExtentions
    {
        public static LocalizationListResourceView ToLocalizationListResourceView(this LocalizationListResource from)
        {
            var to = from.TranslateTo<LocalizationListResourceView>();
            if (from.ListItems.Count() > 0)
            {
                to.ListItems = new List<ListItemView>();

                foreach (var li in from.ListItems)
                {
                    to.ListItems.Add(li.TranslateTo<ListItemView>());
                }
            }
            return to;
        }
    }
}