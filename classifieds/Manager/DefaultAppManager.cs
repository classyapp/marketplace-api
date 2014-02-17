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
                ExternalMediaThumbnailSizes = new Point[] { new Point { X = 266, Y = 266 } , new Point { X = 301, Y = 301 }, new Point { X = 400, Y = 400 } },
                DefaultProfileImage = "http://d107oye3n9eb07.cloudfront.net/profile_img_69064",
                DefaultProfileThumbnail = "http://d107oye3n9eb07.cloudfront.net/profile_thumb_69064",
                PagingPages = 5,
                PagingPageSize = 12
            };
        }
    }
}