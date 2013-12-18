using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class Variant
    {
        public string Title { get; set; }
        public string Value { get; set; }
    }

    public class PurchaseOption
    {
        public IList<Variant> Variants { get; set; }
        public string SKU { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
    }
}
