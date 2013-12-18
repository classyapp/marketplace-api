using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class SellerView
    {
        public string Category { get; set; }
        public string TaxId { get; set; }
        public string DisplayName { get; set; }
        public string LegalEntityName { get; set; }
        public ExtendedContactInfoView ContactInfo { get; set; }
        public int SettlementPeriodInDays { get; set; }
        public int RollingReservePercent { get; set; }
        public int RollingReserveTimeInDays { get; set; }
    }
}