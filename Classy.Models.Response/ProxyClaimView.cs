using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class ProxyClaimView
    {
        public string Id { get; set; }
        public string ProfileId { get; set; }
        public string ProxyProfileId { get; set; }
        public SellerView SellerInfo { get; set; }
        public IList<CustomAttributeView> Metadata { get; set; }
        public int Status { get; set; }
    }
}