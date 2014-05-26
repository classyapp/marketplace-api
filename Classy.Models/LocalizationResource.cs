using System.Collections.Generic;
using Classy.Models.Attributes;

namespace Classy.Models
{
    [MongoCollection(Name = "resources")]
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
}
