using Classy.Models.Attributes;

namespace Classy.Models
{
    [MongoCollection(Name = "comments")]
    public class Comment : BaseObject
    {
        public string ObjectId { get; set; }
        public string ProfileId { get; set; }
        public string Content { get; set; }
    }
}