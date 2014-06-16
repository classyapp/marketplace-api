using MongoDB.Bson.Serialization.Attributes;
using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Classy.Models
{
    [BsonIgnoreExtraElements]
    public class PricingInfo
    {
        // Product configurations - at least one
        public IList<PurchaseOption> PurchaseOptions { get; set; }

        public string CurrencyCode { get; set; }
        public PurchaseOption BaseOption { get; set; }

        // TODO: make this a list of shipping options? 
        //public int? DomesticRadius { get; set; }
        //public decimal? DomesticShippingPrice { get; set; }
        //public decimal? InternationalShippingPrice { get; set; }

        public double GetPriceForSKU(string sku)
        {
            var option = PurchaseOptions.SingleOrDefault(x => x.SKU == sku);
            if (option != null) return option.Price;
            throw new ApplicationException("invalid SKU");
        }
    }

    public class PricingInfoValidator : AbstractValidator<PricingInfo>
    {
        public PricingInfoValidator()
        {
            RuleFor(x => x.BaseOption).NotNull();
            RuleFor(x => x.CurrencyCode).NotEmpty();
        }
    }
}