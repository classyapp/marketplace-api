using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class PurchaseOption
    {
        public string VariantKey { get; set; } // Size, Color, Model, etc.
        public string VariantValue { get; set; } // S, M, L, Red, Blue, Pink/Crimson, etc.
        public string SKU { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
    }
}
