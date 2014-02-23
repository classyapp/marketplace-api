using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class SocialPhotoAlbumView
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhotoCount { get; set; }
        public string CoverPhoto { get; set; }
        public IList<SocialPhotoView> Photos { get; set; }
    }
}
