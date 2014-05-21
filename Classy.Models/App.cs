using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Classy.Models
{
    public class App : BaseObject, ITranslatable<App>
    {
        public int PageSize { get; set; }
        public int PagesCount { get; set; }
        public bool AllowUnmoderatedReviews { get; set; }
        public bool AllowUnmoderatedComments { get; set; }
        public bool EnableProxyProfiles { get; set; }
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
        public string MandrilAPIKey { get; set; }
        public int ImageReducedSize { get; set; }
        public string DefaultFromEmailAddress { get; set; }

        public IndexingInfo IndexingInfo { get; set; }

        // Static lists of values
        public IList<CurrencyListItem> SupportedCurrencies { get; set; }
        public IList<ListItem> SupportedCultures { get; set; }
        public IList<ListItem> SupportedCountries { get; set; }

        public App Translate(string culture)
        {
            return this;
        }
    }

    public class IndexingInfo
    {
        public string[] ListingTypes { get; set; }
        // I think we can depend only on the dictionary
        public Dictionary<string, string[]> MetadataPerListing { get; set; }
    }
}