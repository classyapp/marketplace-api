using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models.Response
{
    public class PricingInfoView
    {
        public IList<PurchaseOptionView> PurchaseOptions { get; set; }
    }
}
