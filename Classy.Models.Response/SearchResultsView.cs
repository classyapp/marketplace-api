using System.Collections.Generic;

namespace Classy.Models.Response
{
    public class SearchResultsView<T>
    {
        public IList<T> Results { get; set; }
        public long Count { get; set; }
    }
}
