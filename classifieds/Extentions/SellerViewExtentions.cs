using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class SellerViewExtentions
    {
        public static SellerView ToSellerView(this Seller from)
        {
            var to = from.TranslateTo<SellerView>();
            to.ContactInfo = from.ContactInfo.ToExtendedContactInfoView();
            return to;
        }
    }
}