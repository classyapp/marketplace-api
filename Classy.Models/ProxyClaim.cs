using System.Collections.Generic;
using Classy.Models.Attributes;

namespace Classy.Models
{
    public enum ProxyClaimStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    [MongoCollection(Name = "proxyclaims")]
    public class ProxyClaim : BaseObject
    {
        public string ProfileId { get; set; }
        public string ProxyProfileId { get; set; }
        public ProfessionalInfo ProfessionalInfo { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ProxyClaimStatus Status { get; set; }
        public string DefaultCulture { get; set; }
    }
}