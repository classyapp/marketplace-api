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

        public double GetPriceForSKU(string sku)
        {
            var option = PurchaseOptions.SingleOrDefault(x => x.SKU == sku);
            if (option != null) return option.Price;
            throw new ApplicationException("invalid SKU");
        }

        public PurchaseOption FindByVariation(Dictionary<string, string> dictionary)
        {
            IEnumerable<PurchaseOption> options = null;
            foreach (var key in dictionary.Keys)
            {
                if (options == null)
                {
                    options = this.PurchaseOptions.Where(po => po.VariantProperties.Contains(new KeyValuePair<string, string>(key, dictionary[key])));
                }
                else
                {
                    options = options.Where(po => po.VariantProperties.Contains(new KeyValuePair<string, string>(key, dictionary[key])));
                }
            }

            return (options == null ? null : options.First());
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