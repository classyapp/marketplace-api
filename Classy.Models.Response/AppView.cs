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
        public bool EnableProxyProfiles { get; set; }
        public bool ProxyClaimNeedsVerification { get; set; }
        public string DefaultProfileImage { get; set; }
    }
}