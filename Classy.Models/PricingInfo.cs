using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class PricingInfo
    {
        // pricing
        public string SKU { get; set; }
        public double? Price { get; set; }
        public double? CompareAtPrice { get; set; }

        // inventory
        public int Quantity { get; set; }

        // purchase options (size, color, etc)
        public IList<PurchaseOption> PurchaseOptions { get; set; }

        // TODO: make this a list of shipping options? 
        public int? DomesticRadius { get; set; }
        public decimal? DomesticShippingPrice { get; set; }
        public decimal? InternationalShippingPrice { get; set; }

        //
        public double GetPriceForSKU(string sku)
        {
            if (SKU == sku) return Price.Value;
            else if (PurchaseOptions != null)
            {
                var option = PurchaseOptions.SingleOrDefault(x => x.SKU == sku);
                if (option != null) return option.Price;
            }
            throw new ApplicationException("invalid SKU");
        }
    }

    public class PricingInfoValidator : AbstractValidator<PricingInfo>
    {
        public PricingInfoValidator()
        {
            RuleFor(x => x.SKU).NotEmpty();
            RuleFor(x => x.Price).NotNull();
        }
    }
}
