using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    public class Translation
    {
        public string Culture { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class ProfileTranslation : Translation
    {
    }

    public class CollectionTranslation : Translation
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class ListingTranslation : Translation
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class IncludedListingTranslation : Translation
    {
        public string Comments { get; set; }
    }
}
