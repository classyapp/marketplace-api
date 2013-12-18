using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class LocationViewExtentions
    {
        public static LocationView ToLocationView(this Location from)
        {
            var to = from.TranslateTo<LocationView>();
            if (from.Address != null) to.Address = from.Address.TranslateTo<PhysicalAddressView>();
            return to;
        }
    }
}