using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using Classy.Models.Response;
using ServiceStack.Common;

namespace classy
{
    public static class AppViewExtensions
    {
        public static AppView ToAppView(this App from)
        {
            var to = from.TranslateTo<AppView>();
            to.SupportedCountries = from.SupportedCountries.Select(v => v.TranslateTo<ListItemView>()).ToList();
            to.SupportedCultures = from.SupportedCultures.Select(v => v.TranslateTo<ListItemView>()).ToList();
            to.SupportedCurrencies = from.SupportedCurrencies.Select(v => v.TranslateTo<CurrencyListItemView>()).ToList();
            to.ProductCategories = from.ProductCategories.Select(v => v.TranslateTo<ListItemView>()).ToList();
            return to;
        }
    }
}