using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class ProfessionalInfoView
    {
        public string Category { get; set; }
        public string TaxId { get; set; }
        public string CompanyName { get; set; }
        public ExtendedContactInfoView CompanyContactInfo { get; set; }
        public int SettlementPeriodInDays { get; set; }
        public int RollingReservePercent { get; set; }
        public int RollingReserveTimeInDays { get; set; }
    }
}