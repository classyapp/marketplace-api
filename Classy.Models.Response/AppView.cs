﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class AppView
    {
        public int PageSize { get; set; }
        public int PagesCount { get; set; }
        public bool AllowUnmoderatedReviews { get; set; }
        public bool AllowUnmoderatedComments { get; set; }
        public bool ProxyClaimNeedsVerification { get; set; }
        public string DefaultProfileImage { get; set; }
        public string DefaultCountry { get; set; }
        public string DefaultCulture { get; set; }
        public string DefaultCurrency { get; set; }
        public string GPSLocationCookieName { get; set; }
        public string GPSOriginCookieName { get; set; }
        public string CountryCookieName { get; set; }
        public string CultureCookieName { get; set; }
        public string CurrencyCookieName { get; set; }
        public string Hostname { get; set; }

        public IList<CurrencyListItemView> SupportedCurrencies { get; set; }
        public IList<ListItemView> SupportedCultures { get; set; }
        public IList<ListItemView> SupportedCountries { get; set; }
        public IList<ListItemView> ProductCategories { get; set; }
    }
}
