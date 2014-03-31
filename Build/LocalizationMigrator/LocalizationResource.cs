using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationMigrator
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

    public class LocalizationResource : BaseObject
    {
        /// <summary>
        /// the resoruce key.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// dictionary of values. the key for the dictionary is the culture name.
        /// </summary>
        public IDictionary<string, string> Values { get; set; }
        public string Description { get; set; }
        public string ContextScreenshotUrl { get; set; }
        public string ContextUrl { get; set; }
    }

    public class ListItem
    {
        public string Value { get; set; }
        public string ParentValue { get; set; }
        public IDictionary<string, string> Text { get; set; }
    }

    public class LocalizationListResource : BaseObject
    {
        /// <summary>
        /// the resoruce key.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// dictionary of values. the key for the dictionary is the culture name.
        /// </summary>
        public IList<ListItem> ListItems { get; set; }
    }
}
