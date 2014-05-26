using Classy.Models.Attributes;

namespace Classy.Models.Keywords
{
    [MongoCollection(Name = "keywords")]
    public class Keyword : BaseObject
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public int Count { get; set; }
    }
}