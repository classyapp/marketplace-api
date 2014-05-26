using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace Classy.Models
{
    /// <summary>
    /// the base class for all model objects
    /// </summary>
    public class BaseObject
    {
        /// <summary>
        /// the database id of the object
        /// </summary>
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        /// <summary>
        /// the id of the application the object belongs to
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// a timestamp of the creation of the object. defaults to UtcNow
        /// </summary>
        public DateTime Created { get; set; }

        public BaseObject()
        {
            Created = DateTime.UtcNow;
        }
    }
}