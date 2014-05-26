using System.Collections.Generic;
using Classy.Models.Attributes;

namespace Classy.Models
{
    public class ListItem
    {
        public string Value { get; set; }
        public string ParentValue { get; set; }
        public IDictionary<string, string> Text { get; set; }
    }

    [MongoCollection(Name = "listresources")]
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
