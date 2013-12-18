using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models
{
    public enum ProxyClaimStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public class ProxyClaim : BaseObject
    {
        public string ProfileId { get; set; }
        public string ProxyProfileId { get; set; }
        public Seller SellerInfo { get; set; }
        public IList<CustomAttribute> Metadata { get; set; }
        public ProxyClaimStatus Status { get; set; }
    }
}