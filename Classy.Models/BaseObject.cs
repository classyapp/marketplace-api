using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models
{
    public class BaseObject
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string AppId { get; set; }
        public DateTime Created { get; set; }

        public BaseObject()
        {
            Created = DateTime.UtcNow;
        }
    }
}