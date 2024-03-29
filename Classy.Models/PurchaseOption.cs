using System.Collections.Generic;
using Classy.Models.Serializers;
using MongoDB.Bson.Serialization.Attributes;

namespace Classy.Models
{
    [BsonIgnoreExtraElements]
    public class PurchaseOption
    {
        public string Title{ get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> VariantProperties { get; set; } // Key: Size, Color, Model, etc. Value: Smal, Medium, Large, etc.
        public string SKU { get; set; }
        [BsonSerializer(typeof(MongoDbMoneyFieldSerializer))]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public MediaFile[] MediaFiles { get; set; }
        public string ProductUrl { get; set; }
        [BsonSerializer(typeof(MongoDbMoneyFieldSerializer))]
        public decimal NeutralPrice { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Depth { get; set; }
        public bool Available { get; set; }
        public string UID { get; set; }
    }
}