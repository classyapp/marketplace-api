using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models
{
    public class ProfessionalInfo
    {
        public ProfessionalInfo()
        {
            SettlementPeriodInDays = 45;
            RollingReservePercent = 10;
            RollingReserveTimeInDays = 90;
            CompanyContactInfo = new ContactInfo();
        }

        public string Category { get; set; }
        public string TaxId { get; set; }
        public string CompanyName { get; set; }
        public ContactInfo CompanyContactInfo { get; set; }
        public BankAccount PaymentDetails { get; set; }
        public int SettlementPeriodInDays { get; set; }
        public int RollingReservePercent { get; set; }
        public int RollingReserveTimeInDays { get; set; }
    }
}