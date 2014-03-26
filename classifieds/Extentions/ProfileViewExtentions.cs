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
            if (from.ProfessionalInfo != null)
            {
                to.IsVerifiedProfessional = from.IsVerifiedProfessional;
                to.IsVendor = from.IsVendor;
                to.IsProfessional = from.IsProfessional;
                to.ProfessionalInfo = from.ProfessionalInfo.ToSellerView();
                to.IsFacebookConnected = !from.FacebookUserId.IsNullOrEmpty();
                to.IsGoogleConnected = !from.GoogleUserName.IsNullOrEmpty();
            }
            // avatar
            if (from.Avatar != null) to.Avatar = from.Avatar.ToMediaFileView();
            //proxy
            to.IsProxy = from.IsProxy;

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

        public static void MergeTranslation(this ProfileView profileView, Translation translation)
        {
            foreach (var key in translation.Metadata.Keys)
            {
                profileView.Metadata[key] = translation.Metadata[key];
            }
        }
    }
}