using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class ProxyClaimViewExtentions
    {
        public static ProxyClaimView ToProxyClaimView(this ProxyClaim from)
        {
            var to = from.TranslateTo<ProxyClaimView>();
            to.SellerInfo = from.SellerInfo.ToSellerView();
            to.Metadata = from.Metadata.ToCustomAttributeViewList();
            return to;
        }
    }
}