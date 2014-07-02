using System;
using System.Collections.Generic;
using System.Linq;
using Classy.Models.Attributes;

namespace Classy.Models
{
    public class SubTransaction
    {
        public DateTime Created { get; set; }
        public decimal Amount { get; set; }
        public string GatewayRefId { get; set; }
    }

    [MongoCollection(Name = "transactions")]
    public class Transaction : BaseObject
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string GatewayRefId { get; set; }
        public SubTransaction Capture { get; set; }
        public IList<SubTransaction> Refunds { get; set; }

        public bool IsCaptured
        {
            get
            {
                return this.Capture != null;
            }
        }

        public bool CanRefund
        {
            get
            {
                if (Refunds == null) return true;
                else return Refunds.Sum(x => x.Amount) < this.Amount;
            }
        }
    }
}