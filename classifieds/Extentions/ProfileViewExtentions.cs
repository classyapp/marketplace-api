using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class ProfileViewExtentions
    {
        public static ProfileView ToProfileView(this Profile from)
        {
            var to = from.TranslateTo<ProfileView>();
            // contact info
            to.ContactInfo = from.ContactInfo.ToExtendedContactInfoView();
            // add merchant info
            if (from.SellerInfo != null)
            {
                to.IsVerified = from.IsVerifiedSeller;
                to.IsSeller = from.IsSeller;
                to.SellerInfo = from.SellerInfo.ToSellerView();
            }
            //proxy
            to.IsProxy = from.IsProxy;
            //metadata
            to.Metadata = from.Metadata.ToCustomAttributeViewList();
            return to;
        }

        public static IList<ProfileView> ToProfileViewList(this IList<Profile> from)
        {
            var to = new List<ProfileView>();
            foreach(var p in from)
            {
                to.Add(p.ToProfileView());
            }
            return to;
        }
    }
}