using MongoDB.Bson.Serialization.Attributes;

namespace Classy.Models
{
    [BsonIgnoreExtraElements]
    public class PhysicalAddress
    {
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }
}