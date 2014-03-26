using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    /// <summary>
    /// Holds a pointer to a listing and comments
    /// </summary>
    public class IncludedListing : ITranslatable<IncludedListing>
    {
        public string Id { get; set; }
        public string Comments { get; set; }
        public string ListingType { get; set; }

        public IDictionary<string, IncludedListingTranslation> Translations { get; set; }

        public IncludedListing Translate(string culture)
        {
            if (Translations != null && !string.IsNullOrEmpty(culture))
            {
                IncludedListingTranslation translation = null;
                if (Translations.TryGetValue(culture, out translation))
                {
                    Comments = string.IsNullOrEmpty(translation.Comments) ? Comments : translation.Comments;
                }
            }

            return this;
        }
    }
}
