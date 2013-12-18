using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class CustomAttribute
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public static class CustomAttributeExtenstions
    {
        public static bool ContainsKey(this IList<CustomAttribute> attributes, string key) {
            return attributes.AsQueryable().Any(x => x.Key == key);
        }

        public static void SetValue(this IList<CustomAttribute> attributes, string key, string value)
        {
            var attribute = attributes.SingleOrDefault(x => x.Key == key);
            if (attribute == null) attributes.Add(new CustomAttribute { Key = key, Value = value });
            else attribute.Value = value;
        }
    }
}
