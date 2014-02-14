using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class SearchResultsView<T>
    {
        public IList<T> Results { get; set; }
        public long Count { get; set; }
    }
}
