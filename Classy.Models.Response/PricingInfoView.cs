using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models.Response
{
    public class PricingInfoView
    {
        // pricing
        public string SKU { get; set; }
        public double? Price { get; set; }
        public double? CompareAtPrice { get; set; }

        // inventory
        public int Quantity { get; set; }

        // purchase options (size, color, etc)
        public IList<PurchaseOptionView> PurchaseOptions { get; set; }

        // TODO: make this a list of shipping options? 
        public int? DomesticRadius { get; set; }
        public decimal? DomesticShippingPrice { get; set; }
        public decimal? InternationalShippingPrice { get; set; }
    }
}
