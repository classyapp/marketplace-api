using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    [Flags]
    public enum MediaFileType
    {
        Image = 1 << 0
    }

    public class MediaFile
    {
        public MediaFile()
        {
            Thumbnails = new List<MediaThumbnail>();
        }

        public MediaFileType Type { get; set; }
        public string Key { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public IList<MediaThumbnail> Thumbnails { get; set; }
    }
}
