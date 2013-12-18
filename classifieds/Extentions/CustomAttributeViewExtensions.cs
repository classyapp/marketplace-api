using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy
{
    public static class CustomAttributeViewExtentions
    {
        public static IList<CustomAttributeView> ToCustomAttributeViewList(this IList<CustomAttribute> from)
        {
            if (from == null) return null;
        
            var to = new List<CustomAttributeView>();
            foreach(var a in from)
            {
                to.Add(a.TranslateTo<CustomAttributeView>());
            }
            return to;
        }
    }
}