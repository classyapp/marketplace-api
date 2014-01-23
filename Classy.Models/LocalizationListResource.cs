using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    public class ListItem
    {
        public string Value { get; set; }
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
