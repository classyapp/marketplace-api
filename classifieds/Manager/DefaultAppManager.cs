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
                ExternalMediaThumbnailSizes = new Point[] { new Point { X = 266, Y = 266 } , new Point { X = 301, Y = 301 }, new Point { X = 400, Y = 400 } }
            };
        }
    }
}