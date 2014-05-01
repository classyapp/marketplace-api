using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class ListingMoreInfoView
    {
        public string CollectionType { get; set; }
        public IList<ListingView> CollectionLisitngs { get; set; }
        public IList<CollectionView> Collections { get; set; }
    }
}
