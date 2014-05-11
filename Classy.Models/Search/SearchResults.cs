using System.Collections.Generic;

namespace Classy.Models.Search
{
    public class SearchResults<T> where T : class
    {
        public int TotalResults { get; set; }
        public IList<T> Results { get; set; }
    }
}