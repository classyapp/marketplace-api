using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models.Response
{
    public class MediaFileView
    {
        public string Key { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public IList<MediaThumbnailView> Thumbnails { get; set; }
    }
}
