using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models
{
    public class Seller
    {
        public Seller()
        {
            SettlementPeriodInDays = 45;
            RollingReservePercent = 10;
            RollingReserveTimeInDays = 90;
            ContactInfo = new ContactInfo();
        }

        public string Category { get; set; }
        public string TaxId { get; set; }
        public string DisplayName { get; set; }
        public string LegalEntityName { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public BankAccount PaymentDetails { get; set; }
        public int SettlementPeriodInDays { get; set; }
        public int RollingReservePercent { get; set; }
        public int RollingReserveTimeInDays { get; set; }
    }
}