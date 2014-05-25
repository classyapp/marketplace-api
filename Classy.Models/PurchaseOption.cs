using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Classy.Models
{
    [BsonIgnoreExtraElements]
    public class PurchaseOption
    {
        public string Title{ get; set; }
        public Dictionary<string, string> VariantProperties { get; set; } // Key: Size, Color, Model, etc. Value: Smal, Medium, Large, etc.
        public string SKU { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public double? CompareAtPrice { get; set; }
        public MediaFile[] MediaFiles { get; set; }
        public string DefaultImage { get; set; }
    }
}