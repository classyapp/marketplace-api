using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    /// <summary>
    /// Holds a pointer to a listing and comments
    /// </summary>
    public class IncludedListingView
    {
        public string ListingId { get; set; }
        public string Comments { get; set; }
    }
}
