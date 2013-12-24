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
        public ProfessionalInfo ProfessionalInfo { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ProxyClaimStatus Status { get; set; }
    }
}