using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Classy.Models
{
    public class App : BaseObject
    {
        public bool AllowUnmoderatedReviews { get; set; }
        public bool AllowUnmoderatedComments { get; set; }
        public bool EnableProxyProfiles { get; set; }
        public bool ProxyClaimNeedsVerification { get; set; }
        public Point[] ExternalMediaThumbnailSizes { get; set; }
        public string DefaultProfileImage { get; set; }
        public string DefaultProfileThumbnail { get; set; }
    }
}