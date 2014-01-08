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
    [Flags]
    public enum ImageSize
    {
        None = 1 << 0,
        Original = 1 << 1,
        Thumbnail266x266 = 1 << 2
    }

    public class MediaFile
    {
        public string Key { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public MediaFileType Type { get; set; }
        public ImageSize ImageSize { get; set; }
    }
}
