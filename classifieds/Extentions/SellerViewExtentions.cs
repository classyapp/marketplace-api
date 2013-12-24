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
        public static ProfessionalInfoView ToSellerView(this ProfessionalInfo from)
        {
            var to = from.TranslateTo<ProfessionalInfoView>();
            to.CompanyContactInfo = from.CompanyContactInfo.ToExtendedContactInfoView();
            return to;
        }
    }
}