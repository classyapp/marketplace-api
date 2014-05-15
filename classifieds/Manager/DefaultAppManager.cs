using Classy.Models;
using System.Collections.Generic;

namespace classy.Manager
{
    public class DefaultAppManager : IAppManager
    {
        public App GetAppById(string appId)
        {
            return new App {
                AppId = appId,
                EnableProxyProfiles = true,
                ProxyClaimNeedsVerification = true,
                AllowUnmoderatedComments = true,
                AllowUnmoderatedReviews = true,
                DefaultProfileImage = "//d107oye3n9eb07.cloudfront.net/profile_img_69064",
                PagesCount = 5,
                PageSize = 12,
                DefaultCountry = "FR",
                DefaultCulture = "en",
                GPSLocationCookieName = "classy.env.gps_location",
                GPSOriginCookieName = "classy.env.gps_origin",
                CountryCookieName = "classy.env.country",
                CultureCookieName = "classy.env.culture",
                Hostname = "www.homelab.com",
                MandrilAPIKey = "ndg42WcyRHVLtLbvGqBjUA",
                ImageReducedSize = 1600,
                DefaultFromEmailAddress = "team@homelab.com",
                IndexingInfo = new IndexingInfo {
                    ListingTypes = new[] {"Photo"},
                    MetadataPerListing = new Dictionary<string, string[]> {
                        {"Photo", new[] {"Room", "Style"}}
                    }
                }
            };
        }
    }
}