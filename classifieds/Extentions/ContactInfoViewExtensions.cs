using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class ContactInfoViewExtentions
    {
        public static ExtendedContactInfoView ToExtendedContactInfoView(this ContactInfo from)
        {
            var to = from.TranslateTo<ExtendedContactInfoView>();
            if (from.Location != null) to.Location = from.Location.ToLocationView();
            return to;
        }
    }
}