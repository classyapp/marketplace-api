using Classy.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace classy.Manager
{
    public class DefaultAppManager : IAppManager
    {
        public App GetAppById(string appId)
        {
            return new App
            {
                AppId = appId,
                EnableProxyProfiles = true,
                ProxyClaimNeedsVerification = true,
                AllowUnmoderatedComments = true,
                AllowUnmoderatedReviews = true,
                DefaultProfileImage = "http://d107oye3n9eb07.cloudfront.net/profile_img_69064",
                PagesCount = 5,
                PageSize = 12,
                DefaultCountry = "FR",
                DefaultCulture = "en",
                GPSLocationCookieName = "classy.env.gps_location",
                CountryCookieName = "classy.env.country",
                CultureCookieName = "classy.env.culture"
            };
        }
    }
}